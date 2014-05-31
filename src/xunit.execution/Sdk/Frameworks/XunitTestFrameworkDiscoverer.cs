using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The implementation of <see cref="ITestFrameworkDiscoverer"/> that supports discovery
    /// of unit tests linked against xunit.core.dll, using xunit.execution.dll.
    /// </summary>
    public class XunitTestFrameworkDiscoverer : TestFrameworkDiscoverer
    {
        /// <summary>
        /// Gets the display name of the xUnit.net v2 test framework.
        /// </summary>
        public static readonly string DisplayName = String.Format(CultureInfo.InvariantCulture, "xUnit.net {0}", typeof(XunitTestFrameworkDiscoverer).GetTypeInfo().Assembly.GetName().Version);

        readonly Dictionary<Type, IXunitTestCaseDiscoverer> discovererCache = new Dictionary<Type, IXunitTestCaseDiscoverer>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestFrameworkDiscoverer"/> class.
        /// </summary>
        /// <param name="assemblyInfo">The test assembly.</param>
        /// <param name="sourceProvider">The source information provider.</param>
        public XunitTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider)
            : this(assemblyInfo, sourceProvider, null, null) { }

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
            : base(assemblyInfo, sourceProvider, messageAggregator)
        {
            var collectionBehaviorAttribute = assemblyInfo.GetCustomAttributes(typeof(CollectionBehaviorAttribute)).SingleOrDefault();
            var disableParallelization = collectionBehaviorAttribute == null ? false : collectionBehaviorAttribute.GetNamedArgument<bool>("DisableTestParallelization");

            TestCollectionFactory = collectionFactory ?? ExtensibilityPointFactory.GetXunitTestCollectionFactory(collectionBehaviorAttribute, AssemblyInfo);
            TestFrameworkDisplayName = String.Format("{0} [{1}, {2}]",
                                                     DisplayName,
                                                     TestCollectionFactory.DisplayName,
                                                     disableParallelization ? "non-parallel" : "parallel");
        }

        /// <summary>
        /// Gets the test collection factory that makes test collections.
        /// </summary>
        public IXunitTestCollectionFactory TestCollectionFactory { get; private set; }

        /// <summary>
        /// Finds the tests on a test method.
        /// </summary>
        /// <param name="testCollection">The test collection that the test method belongs to.</param>
        /// <param name="type">The test class that the test method belongs to.</param>
        /// <param name="method">The test method.</param>
        /// <param name="includeSourceInformation">Set to <c>true</c> to indicate that source information should be included.</param>
        /// <param name="messageBus">The message bus to report discovery messages to.</param>
        /// <returns>Return <c>true</c> to continue test discovery, <c>false</c>, otherwise.</returns>
        protected virtual bool FindTestsForMethod(ITestCollection testCollection, ITypeInfo type, IMethodInfo method, bool includeSourceInformation, IMessageBus messageBus)
        {
            var factAttribute = method.GetCustomAttributes(typeof(FactAttribute)).FirstOrDefault();
            if (factAttribute == null)
                return true;

            var testCaseDiscovererAttribute = factAttribute.GetCustomAttributes(typeof(XunitTestCaseDiscovererAttribute)).FirstOrDefault();
            if (testCaseDiscovererAttribute == null)
                return true;

            var args = testCaseDiscovererAttribute.GetConstructorArguments().Cast<string>().ToList();
            var discovererType = Reflector.GetType(args[1], args[0]);
            if (discovererType == null)
                return true;

            var discoverer = GetDiscoverer(discovererType);
            if (discoverer == null)
                return true;

            foreach (var testCase in discoverer.Discover(testCollection, AssemblyInfo, type, method, factAttribute))
                if (!ReportDiscoveredTestCase(testCase, includeSourceInformation, messageBus))
                    return false;

            return true;
        }

        /// <inheritdoc/>
        protected override bool FindTestsForType(ITypeInfo type, bool includeSourceInformation, IMessageBus messageBus)
        {
            var testCollection = TestCollectionFactory.Get(type);

            foreach (var method in type.GetMethods(includePrivateMethods: true))
                if (!FindTestsForMethod(testCollection, type, method, includeSourceInformation, messageBus))
                    return false;

            return true;
        }

        /// <summary>
        /// Gets the test case discover instance for the given discoverer type. The instances are cached
        /// and reused, since they should not be stateful.
        /// </summary>
        /// <param name="discovererType">The discoverer type.</param>
        /// <returns>Returns the test case discoverer instance.</returns>
        protected IXunitTestCaseDiscoverer GetDiscoverer(Type discovererType)
        {
            IXunitTestCaseDiscoverer result;

            if (!discovererCache.TryGetValue(discovererType, out result))
            {
                try
                {
                    result = ExtensibilityPointFactory.GetXunitTestCaseDiscoverer(discovererType);
                }
                catch (Exception ex)
                {
                    result = null;
                    Aggregator.Add(new EnvironmentalWarning { Message = String.Format("Discoverer type '{0}' could not be created or does not implement IXunitTestCaseDiscoverer: {1}", discovererType.FullName, ex) });
                }

                discovererCache[discovererType] = result;
            }

            return result;
        }
    }
}
