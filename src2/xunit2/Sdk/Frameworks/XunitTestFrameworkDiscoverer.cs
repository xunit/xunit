using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The implementation of <see cref="ITestFrameworkDiscoverer"/> that supports discovery
    /// of unit tests linked against xunit2.dll.
    /// </summary>
    public class XunitTestFrameworkDiscoverer : LongLivedMarshalByRefObject, ITestFrameworkDiscoverer
    {
        /// <summary>
        /// Gets the display name of the xUnit.net v2 test framework.
        /// </summary>
        public static readonly string DisplayName = String.Format(CultureInfo.InvariantCulture, "xUnit.net {0}", typeof(XunitTestFrameworkDiscoverer).Assembly.GetName().Version);

        readonly IAssemblyInfo assemblyInfo;
        readonly ISourceInformationProvider sourceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestFrameworkDiscoverer"/> class.
        /// </summary>
        /// <param name="assemblyInfo">The test assembly.</param>
        /// <param name="sourceProvider">The source information provider.</param>
        public XunitTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider)
        {
            Guard.ArgumentNotNull("assemblyInfo", assemblyInfo);
            Guard.ArgumentNotNull("sourceProvider", sourceProvider);

            this.assemblyInfo = assemblyInfo;
            this.sourceProvider = sourceProvider;
        }

        /// <inheritdoc/>
        public string TestFrameworkDisplayName
        {
            get { return DisplayName; }
        }

        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
        public void Find(bool includeSourceInformation, IMessageSink messageSink)
        {
            Guard.ArgumentNotNull("messageSink", messageSink);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                // REVIEW: Hard coding the knowledge that assembly == test collection
                var testCollection = new XunitTestCollection { DisplayName = "Test collection for " + Path.GetFileName(assemblyInfo.AssemblyPath) };

                foreach (var type in assemblyInfo.GetTypes(includePrivateTypes: false))
                    if (!FindImpl(testCollection, type, includeSourceInformation, messageSink))
                        break;

                messageSink.OnMessage(new DiscoveryCompleteMessage());
            });
        }

        /// <inheritdoc/>
        public void Find(string typeName, bool includeSourceInformation, IMessageSink messageSink)
        {
            Guard.ArgumentNotNullOrEmpty("typeName", typeName);
            Guard.ArgumentNotNull("messageSink", messageSink);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                // REVIEW: Hard coding the knowledge that assembly == test collection
                var testCollection = new XunitTestCollection { DisplayName = "Test collection for " + Path.GetFileName(assemblyInfo.AssemblyPath) };

                ITypeInfo typeInfo = assemblyInfo.GetType(typeName);
                if (typeInfo != null)
                    FindImpl(testCollection, typeInfo, includeSourceInformation, messageSink);

                messageSink.OnMessage(new DiscoveryCompleteMessage());
            });
        }

        /// <summary>
        /// Core implementation to discover unit tests in a given test class.
        /// </summary>
        /// <param name="testCollection">The test collection that this type belongs to.</param>
        /// <param name="type">The test class.</param>
        /// <param name="includeSourceInformation">Set to <c>true</c> to attempt to include source information.</param>
        /// <param name="messageSink">The message sink to send discovery messages to.</param>
        /// <returns>Returns <c>true</c> if discovery should continue; <c>false</c> otherwise.</returns>
        protected virtual bool FindImpl(XunitTestCollection testCollection, ITypeInfo type, bool includeSourceInformation, IMessageSink messageSink)
        {
            string currentDirectory = Directory.GetCurrentDirectory();

            try
            {
                if (!String.IsNullOrEmpty(assemblyInfo.AssemblyPath))
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyInfo.AssemblyPath));

                foreach (IMethodInfo method in type.GetMethods(includePrivateMethods: true))
                {
                    IAttributeInfo factAttribute = method.GetCustomAttributes(typeof(FactAttribute)).FirstOrDefault();
                    if (factAttribute != null)
                    {
                        IAttributeInfo discovererAttribute = factAttribute.GetCustomAttributes(typeof(XunitDiscovererAttribute)).FirstOrDefault();
                        if (discovererAttribute != null)
                        {
                            var args = discovererAttribute.GetConstructorArguments().Cast<string>().ToList();
                            var discovererType = Reflector.GetType(args[0], args[1]);
                            if (discovererType != null)
                            {
                                IXunitDiscoverer discoverer = (IXunitDiscoverer)Activator.CreateInstance(discovererType);

                                foreach (XunitTestCase testCase in discoverer.Discover(testCollection, assemblyInfo, type, method, factAttribute))
                                    if (!messageSink.OnMessage(new TestCaseDiscoveryMessage { TestCase = UpdateTestCaseWithSourceInfo(testCase, includeSourceInformation) }))
                                        return false;
                            }
                            // TODO: Figure out a way to report back an error when discovererType is not available
                            // TODO: What if the discovererType can't be created or cast to IXunitDiscoverer?
                            // TODO: Performance optimization: cache instances of the discoverer type
                        }
                    }
                }

                return true;
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDirectory);
            }
        }

        /// <inheritdoc/>
        public string Serialize(ITestCase testCase)
        {
            return SerializationHelper.Serialize(testCase);
        }

        private ITestCase UpdateTestCaseWithSourceInfo(XunitTestCase testCase, bool includeSourceInformation)
        {
            if (includeSourceInformation && sourceProvider != null)
                testCase.SourceInformation = sourceProvider.GetSourceInformation(testCase);

            return testCase;
        }
    }
}