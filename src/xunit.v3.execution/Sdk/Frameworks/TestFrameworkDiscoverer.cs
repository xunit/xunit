using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// A base implementation of <see cref="ITestFrameworkDiscoverer"/> that supports test filtering
    /// and runs the discovery process on a thread pool thread.
    /// </summary>
    public abstract class TestFrameworkDiscoverer : LongLivedMarshalByRefObject, ITestFrameworkDiscoverer
    {
        readonly Lazy<string> targetFramework;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestFrameworkDiscoverer"/> class.
        /// </summary>
        /// <param name="assemblyInfo">The test assembly.</param>
        /// <param name="sourceProvider">The source information provider.</param>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        protected TestFrameworkDiscoverer(IAssemblyInfo assemblyInfo,
                                          ISourceInformationProvider sourceProvider,
                                          IMessageSink diagnosticMessageSink)
        {
            Guard.ArgumentNotNull("assemblyInfo", assemblyInfo);
            Guard.ArgumentNotNull("sourceProvider", sourceProvider);
            Guard.ArgumentNotNull("diagnosticMessageSink", diagnosticMessageSink);

            AssemblyInfo = assemblyInfo;
            DiagnosticMessageSink = diagnosticMessageSink;
            DisposalTracker = new DisposalTracker();
            SourceProvider = sourceProvider;

            targetFramework = new Lazy<string>(() =>
            {
                string result = null;

                var attrib = AssemblyInfo.GetCustomAttributes(typeof(TargetFrameworkAttribute)).FirstOrDefault();
                if (attrib != null)
                    result = attrib.GetConstructorArguments().Cast<string>().First();

                return result ?? "";
            });
        }

        /// <summary>
        /// Gets the assembly that's being discovered.
        /// </summary>
        protected internal IAssemblyInfo AssemblyInfo { get; set; }

        /// <summary>
        /// Gets the message sink used to report diagnostic messages.
        /// </summary>
        protected IMessageSink DiagnosticMessageSink { get; set; }

        /// <summary>
        /// Gets the disposal tracker for the test framework discoverer.
        /// </summary>
        protected DisposalTracker DisposalTracker { get; set; }

        /// <summary>
        /// Get the source code information provider used during discovery.
        /// </summary>
        protected ISourceInformationProvider SourceProvider { get; set; }

        /// <inheritdoc/>
        public string TargetFramework { get { return targetFramework.Value; } }

        /// <inheritdoc/>
        public string TestFrameworkDisplayName { get; protected set; }

        /// <summary>
        /// Implement this method to create a test class for the given CLR type.
        /// </summary>
        /// <param name="class">The CLR type.</param>
        /// <returns>The test class.</returns>
        protected internal abstract ITestClass CreateTestClass(ITypeInfo @class);

        /// <inheritdoc/>
        public void Dispose()
        {
            DisposalTracker.Dispose();
        }

        /// <inheritdoc/>
        public void Find(bool includeSourceInformation, IMessageSink discoveryMessageSink, ITestFrameworkDiscoveryOptions discoveryOptions)
        {
            Guard.ArgumentNotNull("discoveryMessageSink", discoveryMessageSink);
            Guard.ArgumentNotNull("discoveryOptions", discoveryOptions);

            XunitWorkerThread.QueueUserWorkItem(() =>
            {
                using (var messageBus = CreateMessageBus(discoveryMessageSink, discoveryOptions))
                using (new PreserveWorkingFolder(AssemblyInfo))
                {
                    foreach (var type in AssemblyInfo.GetTypes(false).Where(IsValidTestClass))
                    {
                        var testClass = CreateTestClass(type);
                        if (!FindTestsForTypeAndWrapExceptions(testClass, includeSourceInformation, messageBus, discoveryOptions))
                            break;
                    }

                    messageBus.QueueMessage(new DiscoveryCompleteMessage());
                }
            });
        }

        static IMessageBus CreateMessageBus(IMessageSink messageSink, ITestFrameworkDiscoveryOptions options)
        {
            if (options.SynchronousMessageReportingOrDefault())
                return new SynchronousMessageBus(messageSink);

            return new MessageBus(messageSink);
        }

        /// <inheritdoc/>
        public void Find(string typeName, bool includeSourceInformation, IMessageSink discoveryMessageSink, ITestFrameworkDiscoveryOptions discoveryOptions)
        {
            Guard.ArgumentNotNullOrEmpty("typeName", typeName);
            Guard.ArgumentNotNull("discoveryMessageSink", discoveryMessageSink);
            Guard.ArgumentNotNull("discoveryOptions", discoveryOptions);

            XunitWorkerThread.QueueUserWorkItem(() =>
            {
                using (var messageBus = CreateMessageBus(discoveryMessageSink, discoveryOptions))
                using (new PreserveWorkingFolder(AssemblyInfo))
                {
                    var typeInfo = AssemblyInfo.GetType(typeName);
                    if (typeInfo != null && IsValidTestClass(typeInfo))
                    {
                        var testClass = CreateTestClass(typeInfo);
                        FindTestsForTypeAndWrapExceptions(testClass, includeSourceInformation, messageBus, discoveryOptions);
                    }

                    messageBus.QueueMessage(new DiscoveryCompleteMessage());
                }
            });
        }

        /// <summary>
        /// Core implementation to discover unit tests in a given test class.
        /// </summary>
        /// <param name="testClass">The test class.</param>
        /// <param name="includeSourceInformation">Set to <c>true</c> to attempt to include source information.</param>
        /// <param name="messageBus">The message sink to send discovery messages to.</param>
        /// <param name="discoveryOptions">The options used by the test framework during discovery.</param>
        /// <returns>Returns <c>true</c> if discovery should continue; <c>false</c> otherwise.</returns>
        protected abstract bool FindTestsForType(ITestClass testClass, bool includeSourceInformation, IMessageBus messageBus, ITestFrameworkDiscoveryOptions discoveryOptions);

        bool FindTestsForTypeAndWrapExceptions(ITestClass testClass, bool includeSourceInformation, IMessageBus messageBus, ITestFrameworkDiscoveryOptions discoveryOptions)
        {
            try
            {
                return FindTestsForType(testClass, includeSourceInformation, messageBus, discoveryOptions);
            }
            catch (Exception ex)
            {
                DiagnosticMessageSink.OnMessage(new DiagnosticMessage($"Exception during discovery:{Environment.NewLine}{ex}"));
                return true; // Keep going on to the next type
            }
        }

        /// <summary>
        /// Determines if a type should be used for discovery. Can be used to filter out types that
        /// are not desirable. The default implementation filters out abstract (non-static) classes.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Returns <c>true</c> if the type can contain tests; <c>false</c>, otherwise.</returns>
        protected virtual bool IsValidTestClass(ITypeInfo type)
        {
            return !type.IsAbstract || type.IsSealed;
        }

        /// <summary>
        /// Reports a discovered test case to the message bus, after updating the source code information
        /// (if desired).
        /// </summary>
        /// <param name="testCase"></param>
        /// <param name="includeSourceInformation"></param>
        /// <param name="messageBus"></param>
        /// <returns></returns>
        protected bool ReportDiscoveredTestCase(ITestCase testCase, bool includeSourceInformation, IMessageBus messageBus)
        {
            if (includeSourceInformation && SourceProvider != null)
                testCase.SourceInformation = SourceProvider.GetSourceInformation(testCase);

            return messageBus.QueueMessage(new TestCaseDiscoveryMessage(testCase));
        }

        /// <inheritdoc/>
        public virtual string Serialize(ITestCase testCase)
        {
            return SerializationHelper.Serialize(testCase);
        }

        class PreserveWorkingFolder : IDisposable
        {
#if NETFRAMEWORK
            readonly string originalWorkingFolder;
#endif

            public PreserveWorkingFolder(IAssemblyInfo assemblyInfo)
            {
#if NETFRAMEWORK
                originalWorkingFolder = Directory.GetCurrentDirectory();

                if (!string.IsNullOrEmpty(assemblyInfo.AssemblyPath))
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyInfo.AssemblyPath));
#endif
            }

            public void Dispose()
            {
#if NETFRAMEWORK
                Directory.SetCurrentDirectory(originalWorkingFolder);
#endif
            }
        }
    }
}
