using System;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// A base implementation of <see cref="ITestFrameworkDiscoverer"/> that supports test filtering
    /// and runs the discovery process on a thread pool thread.
    /// </summary>
    public abstract class TestFrameworkDiscoverer : LongLivedMarshalByRefObject, ITestFrameworkDiscoverer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestFrameworkDiscoverer"/> class.
        /// </summary>
        /// <param name="assemblyInfo">The test assembly.</param>
        /// <param name="sourceProvider">The source information provider.</param>
        /// <param name="messageAggregator">The message aggregator to receive environmental warnings from.</param>
        public TestFrameworkDiscoverer(IAssemblyInfo assemblyInfo,
                                       ISourceInformationProvider sourceProvider,
                                       IMessageAggregator messageAggregator)
        {
            Guard.ArgumentNotNull("assemblyInfo", assemblyInfo);
            Guard.ArgumentNotNull("sourceProvider", sourceProvider);

            Aggregator = messageAggregator ?? MessageAggregator.Instance;
            AssemblyInfo = assemblyInfo;
            DisposalTracker = new DisposalTracker();
            SourceProvider = sourceProvider;
        }

        /// <summary>
        /// Gets the message aggregator used to provide environmental warnings.
        /// </summary>
        protected IMessageAggregator Aggregator { get; private set; }

        /// <summary>
        /// Gets the assembly that's being discovered.
        /// </summary>
        protected IAssemblyInfo AssemblyInfo { get; private set; }

        /// <summary>
        /// Gets the disposal tracker for the test framework discoverer.
        /// </summary>
        protected DisposalTracker DisposalTracker { get; private set; }

        /// <summary>
        /// Get the source code information provider used during discovery.
        /// </summary>
        protected ISourceInformationProvider SourceProvider { get; private set; }

        /// <inheritdoc/>
        public string TestFrameworkDisplayName { get; protected set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            DisposalTracker.Dispose();
        }

        /// <inheritdoc/>
        public void Find(bool includeSourceInformation, IMessageSink messageSink, ITestFrameworkOptions options)
        {
            Guard.ArgumentNotNull("messageSink", messageSink);
            Guard.ArgumentNotNull("options", options);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                using (var messageBus = new MessageBus(messageSink))
                using (new PreserveWorkingFolder(AssemblyInfo))
                {
                    foreach (var type in AssemblyInfo.GetTypes(includePrivateTypes: false).Where(IsValidTestClass))
                        if (!FindTestsForTypeAndWrapExceptions(type, includeSourceInformation, messageBus))
                            break;

                    var warnings = Aggregator.GetAndClear<EnvironmentalWarning>().Select(w => w.Message).ToList();
                    messageBus.QueueMessage(new DiscoveryCompleteMessage(warnings));
                }
            });
        }

        /// <inheritdoc/>
        public void Find(string typeName, bool includeSourceInformation, IMessageSink messageSink, ITestFrameworkOptions options)
        {
            Guard.ArgumentNotNullOrEmpty("typeName", typeName);
            Guard.ArgumentNotNull("messageSink", messageSink);
            Guard.ArgumentNotNull("options", options);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                using (var messageBus = new MessageBus(messageSink))
                using (new PreserveWorkingFolder(AssemblyInfo))
                {
                    var typeInfo = AssemblyInfo.GetType(typeName);
                    if (typeInfo != null && IsValidTestClass(typeInfo))
                        FindTestsForTypeAndWrapExceptions(typeInfo, includeSourceInformation, messageBus);

                    var warnings = Aggregator.GetAndClear<EnvironmentalWarning>().Select(w => w.Message).ToList();
                    messageBus.QueueMessage(new DiscoveryCompleteMessage(warnings));
                }
            });
        }

        /// <summary>
        /// Core implementation to discover unit tests in a given test class.
        /// </summary>
        /// <param name="type">The test class.</param>
        /// <param name="includeSourceInformation">Set to <c>true</c> to attempt to include source information.</param>
        /// <param name="messageBus">The message sink to send discovery messages to.</param>
        /// <returns>Returns <c>true</c> if discovery should continue; <c>false</c> otherwise.</returns>
        protected abstract bool FindTestsForType(ITypeInfo type, bool includeSourceInformation, IMessageBus messageBus);

        private bool FindTestsForTypeAndWrapExceptions(ITypeInfo type, bool includeSourceInformation, IMessageBus messageBus)
        {
            try
            {
                return FindTestsForType(type, includeSourceInformation, messageBus);
            }
            catch (Exception ex)
            {
                Aggregator.Add(new EnvironmentalWarning { Message = String.Format("Exception during discovery:{0}{1}", Environment.NewLine, ex) });
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
            readonly string originalWorkingFolder;

            public PreserveWorkingFolder(IAssemblyInfo assemblyInfo)
            {
                originalWorkingFolder = Directory.GetCurrentDirectory();

                if (!String.IsNullOrEmpty(assemblyInfo.AssemblyPath))
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyInfo.AssemblyPath));
            }

            public void Dispose()
            {
                Directory.SetCurrentDirectory(originalWorkingFolder);
            }
        }
    }
}
