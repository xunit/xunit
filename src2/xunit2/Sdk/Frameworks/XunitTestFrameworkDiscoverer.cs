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

            // Determine the collection behavior, and tack it onto the end of the display name
            var collectionBehavior = assemblyInfo.GetCustomAttributes(typeof(CollectionBehaviorAttribute)).SingleOrDefault();
            var factoryType = GetTestCollectionFactoryType(collectionBehavior);

            TestCollectionFactory = (IXunitTestCollectionFactory)Activator.CreateInstance(factoryType, new[] { assemblyInfo });
            TestFrameworkDisplayName = String.Format("{0} [{1}, non-parallel]", DisplayName, TestCollectionFactory.DisplayName);
        }

        private Type GetTestCollectionFactoryType(IAttributeInfo collectionBehavior)
        {
            if (collectionBehavior == null)
                return typeof(CollectionPerClassTestCollectionFactory);

            var ctorArgs = collectionBehavior.GetConstructorArguments().ToList();
            if (ctorArgs.Count == 0)
                return typeof(CollectionPerClassTestCollectionFactory);

            if (ctorArgs.Count == 1 && (CollectionBehavior)ctorArgs[0] == CollectionBehavior.CollectionPerAssembly)
                return typeof(CollectionPerAssemblyTestCollectionFactory);

            var result = Reflector.GetType((string)ctorArgs[0], (string)ctorArgs[1]);
            if (!typeof(IXunitTestCollectionFactory).IsAssignableFrom(result) || result.GetConstructor(new[] { typeof(IAssemblyInfo) }) == null)
                return typeof(CollectionPerClassTestCollectionFactory);

            return result;
        }

        /// <summary>
        /// Gets the test collection factory that makes test collections.
        /// </summary>
        public IXunitTestCollectionFactory TestCollectionFactory { get; private set; }

        /// <inheritdoc/>
        public string TestFrameworkDisplayName { get; private set; }

        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
        public void Find(bool includeSourceInformation, IMessageSink messageSink)
        {
            Guard.ArgumentNotNull("messageSink", messageSink);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                foreach (var type in assemblyInfo.GetTypes(includePrivateTypes: false))
                    if (!FindImpl(type, includeSourceInformation, messageSink))
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
                ITypeInfo typeInfo = assemblyInfo.GetType(typeName);
                if (typeInfo != null)
                    FindImpl(typeInfo, includeSourceInformation, messageSink);

                messageSink.OnMessage(new DiscoveryCompleteMessage());
            });
        }

        /// <summary>
        /// Core implementation to discover unit tests in a given test class.
        /// </summary>
        /// <param name="type">The test class.</param>
        /// <param name="includeSourceInformation">Set to <c>true</c> to attempt to include source information.</param>
        /// <param name="messageSink">The message sink to send discovery messages to.</param>
        /// <returns>Returns <c>true</c> if discovery should continue; <c>false</c> otherwise.</returns>
        protected virtual bool FindImpl(ITypeInfo type, bool includeSourceInformation, IMessageSink messageSink)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            var testCollection = TestCollectionFactory.Get(type);

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