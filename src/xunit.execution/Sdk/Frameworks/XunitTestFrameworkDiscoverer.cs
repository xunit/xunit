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
        public static readonly string DisplayName = string.Format(CultureInfo.InvariantCulture, "xUnit.net {0}", new object[] { typeof(XunitTestFrameworkDiscoverer).GetTypeInfo().Assembly.GetName().Version });

        readonly Dictionary<Type, IXunitTestCaseDiscoverer> discovererCache = new Dictionary<Type, IXunitTestCaseDiscoverer>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestFrameworkDiscoverer"/> class.
        /// </summary>
        /// <param name="assemblyInfo">The test assembly.</param>
        /// <param name="sourceProvider">The source information provider.</param>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="collectionFactory">The test collection factory used to look up test collections.</param>
        public XunitTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo,
                                            ISourceInformationProvider sourceProvider,
                                            IMessageSink diagnosticMessageSink,
                                            IXunitTestCollectionFactory collectionFactory = null)
            : base(assemblyInfo, sourceProvider, diagnosticMessageSink)
        {
            var collectionBehaviorAttribute = assemblyInfo.GetCustomAttributes(typeof(CollectionBehaviorAttribute)).SingleOrDefault();
            var disableParallelization = collectionBehaviorAttribute != null && collectionBehaviorAttribute.GetNamedArgument<bool>("DisableTestParallelization");

            string config = null;
#if !WINDOWS_PHONE_APP && !WINDOWS_PHONE && !DOTNETCORE
            config = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
#endif
            var testAssembly = new TestAssembly(assemblyInfo, config);

            TestCollectionFactory = collectionFactory ?? ExtensibilityPointFactory.GetXunitTestCollectionFactory(diagnosticMessageSink, collectionBehaviorAttribute, testAssembly);
            TestFrameworkDisplayName = $"{DisplayName} [{TestCollectionFactory.DisplayName}, {(disableParallelization ? "non-parallel" : "parallel")}]";
        }

        /// <summary>
        /// Gets the test collection factory that makes test collections.
        /// </summary>
        public IXunitTestCollectionFactory TestCollectionFactory { get; private set; }

        /// <inheritdoc/>
        protected override ITestClass CreateTestClass(ITypeInfo @class)
        {
            return new TestClass(TestCollectionFactory.Get(@class), @class);
        }

        /// <summary>
        /// Finds the tests on a test method.
        /// </summary>
        /// <param name="testMethod">The test method.</param>
        /// <param name="includeSourceInformation">Set to <c>true</c> to indicate that source information should be included.</param>
        /// <param name="messageBus">The message bus to report discovery messages to.</param>
        /// <param name="discoveryOptions">The options used by the test framework during discovery.</param>
        /// <returns>Return <c>true</c> to continue test discovery, <c>false</c>, otherwise.</returns>
        protected virtual bool FindTestsForMethod(ITestMethod testMethod, bool includeSourceInformation, IMessageBus messageBus, ITestFrameworkDiscoveryOptions discoveryOptions)
        {
            var factAttributes = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).CastOrToList();
            if (factAttributes.Count > 1)
            {
                var message = $"Test method '{testMethod.TestClass.Class.Name}.{testMethod.Method.Name}' has multiple [Fact]-derived attributes";
                var testCase = new ExecutionErrorTestCase(DiagnosticMessageSink, TestMethodDisplay.ClassAndMethod, testMethod, message);
                return ReportDiscoveredTestCase(testCase, includeSourceInformation, messageBus);
            }

            var factAttribute = factAttributes.FirstOrDefault();
            if (factAttribute == null)
                return true;

            var testCaseDiscovererAttribute = factAttribute.GetCustomAttributes(typeof(XunitTestCaseDiscovererAttribute)).FirstOrDefault();
            if (testCaseDiscovererAttribute == null)
                return true;

            var args = testCaseDiscovererAttribute.GetConstructorArguments().Cast<string>().ToList();
            var discovererType = SerializationHelper.GetType(args[1], args[0]);
            if (discovererType == null)
                return true;

            var discoverer = GetDiscoverer(discovererType);
            if (discoverer == null)
                return true;

            foreach (var testCase in discoverer.Discover(discoveryOptions, testMethod, factAttribute))
                if (!ReportDiscoveredTestCase(testCase, includeSourceInformation, messageBus))
                    return false;

            return true;
        }

        /// <inheritdoc/>
        protected override bool FindTestsForType(ITestClass testClass, bool includeSourceInformation, IMessageBus messageBus, ITestFrameworkDiscoveryOptions discoveryOptions)
        {
            foreach (var method in testClass.Class.GetMethods(true))
            {
                var testMethod = new TestMethod(testClass, method);
                if (!FindTestsForMethod(testMethod, includeSourceInformation, messageBus, discoveryOptions))
                    return false;
            }

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
                    result = ExtensibilityPointFactory.GetXunitTestCaseDiscoverer(DiagnosticMessageSink, discovererType);
                }
                catch (Exception ex)
                {
                    result = null;
                    DiagnosticMessageSink.OnMessage(new DiagnosticMessage($"Discoverer type '{discovererType.FullName}' could not be created or does not implement IXunitTestCaseDiscoverer: {ex}"));
                }

                discovererCache[discovererType] = result;
            }

            return result;
        }
    }
}
