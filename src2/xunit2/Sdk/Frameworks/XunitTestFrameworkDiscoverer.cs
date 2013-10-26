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
        readonly IMessageAggregator messageAggregator;
        readonly ISourceInformationProvider sourceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestFrameworkDiscoverer"/> class.
        /// </summary>
        /// <param name="assemblyInfo">The test assembly.</param>
        /// <param name="sourceProvider">The source information provider.</param>
        public XunitTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider)
            : this(assemblyInfo, sourceProvider, null, MessageAggregator.Instance) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestFrameworkDiscoverer"/> class.
        /// </summary>
        /// <param name="assemblyInfo">The test assembly.</param>
        /// <param name="sourceProvider">The source information provider.</param>
        /// <param name="collectionFactory">The test collection factory used to look up test collections.</param>
        /// <param name="messageAggregator">The message aggregator to receive environmental warnings from.</param>
        public XunitTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo,
                                            ISourceInformationProvider sourceProvider,
                                            IXunitTestCollectionFactory collectionFactory,
                                            IMessageAggregator messageAggregator)
        {
            Guard.ArgumentNotNull("assemblyInfo", assemblyInfo);
            Guard.ArgumentNotNull("sourceProvider", sourceProvider);

            this.assemblyInfo = assemblyInfo;
            this.sourceProvider = sourceProvider;
            this.messageAggregator = messageAggregator ?? MessageAggregator.Instance;

            var collectionBehaviorAttribute = assemblyInfo.GetCustomAttributes(typeof(CollectionBehaviorAttribute)).SingleOrDefault();
            var disableParallelization = collectionBehaviorAttribute == null ? false : collectionBehaviorAttribute.GetNamedArgument<bool>("DisableTestParallelization");

            TestCollectionFactory = collectionFactory ?? GetTestCollectionFactory(this.assemblyInfo, collectionBehaviorAttribute);
            TestFrameworkDisplayName = String.Format("{0} [{1}, {2}]",
                                                     DisplayName,
                                                     TestCollectionFactory.DisplayName,
                                                     disableParallelization ? "non-parallel" : "parallel");
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

                var warnings = messageAggregator.GetAndClear<EnvironmentalWarning>().Select(w => w.Message).ToList();
                messageSink.OnMessage(new DiscoveryCompleteMessage(warnings));
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

                var warnings = messageAggregator.GetAndClear<EnvironmentalWarning>().Select(w => w.Message).ToList();
                messageSink.OnMessage(new DiscoveryCompleteMessage(warnings));
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
                            var discovererType = Reflector.GetType(args[1], args[0]);
                            if (discovererType != null)
                            {
                                IXunitDiscoverer discoverer = (IXunitDiscoverer)Activator.CreateInstance(discovererType);

                                foreach (XunitTestCase testCase in discoverer.Discover(testCollection, assemblyInfo, type, method, factAttribute))
                                    if (!messageSink.OnMessage(new TestCaseDiscoveryMessage(UpdateTestCaseWithSourceInfo(testCase, includeSourceInformation))))
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

        internal static IXunitTestCollectionFactory GetTestCollectionFactory(IAssemblyInfo assemblyInfo, IAttributeInfo collectionBehaviorAttribute)
        {
            var factoryType = GetTestCollectionFactoryType(collectionBehaviorAttribute);

            return (IXunitTestCollectionFactory)Activator.CreateInstance(factoryType, new[] { assemblyInfo });
        }

        internal static Type GetTestCollectionFactoryType(IAttributeInfo collectionBehavior)
        {
            if (collectionBehavior == null)
                return typeof(CollectionPerClassTestCollectionFactory);

            var ctorArgs = collectionBehavior.GetConstructorArguments().ToList();
            if (ctorArgs.Count == 0)
                return typeof(CollectionPerClassTestCollectionFactory);

            if (ctorArgs.Count == 1 && (CollectionBehavior)ctorArgs[0] == CollectionBehavior.CollectionPerAssembly)
                return typeof(CollectionPerAssemblyTestCollectionFactory);

            var result = Reflector.GetType((string)ctorArgs[1], (string)ctorArgs[0]);
            if (!typeof(IXunitTestCollectionFactory).IsAssignableFrom(result) || result.GetConstructor(new[] { typeof(IAssemblyInfo) }) == null)
                return typeof(CollectionPerClassTestCollectionFactory);

            return result;
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