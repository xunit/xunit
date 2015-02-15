using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The test class runner for xUnit.net v2 tests.
    /// </summary>
    public class XunitTestClassRunner : TestClassRunner<IXunitTestCase>
    {
        readonly IDictionary<Type, object> collectionFixtureMappings;
        readonly IMessageSink diagnosticMessageSink;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestClassRunner"/> class.
        /// </summary>
        /// <param name="testClass">The test class to be run.</param>
        /// <param name="class">The test class that contains the tests to be run.</param>
        /// <param name="testCases">The test cases to be run.</param>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="testCaseOrderer">The test case orderer that will be used to decide how to order the test.</param>
        /// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        /// <param name="collectionFixtureMappings">The mapping of collection fixture types to fixtures.</param>
        public XunitTestClassRunner(ITestClass testClass,
                                    IReflectionTypeInfo @class,
                                    IEnumerable<IXunitTestCase> testCases,
                                    IMessageSink diagnosticMessageSink,
                                    IMessageBus messageBus,
                                    ITestCaseOrderer testCaseOrderer,
                                    ExceptionAggregator aggregator,
                                    CancellationTokenSource cancellationTokenSource,
                                    IDictionary<Type, object> collectionFixtureMappings)
            : base(testClass, @class, testCases, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
            this.collectionFixtureMappings = collectionFixtureMappings;

            ClassFixtureMappings = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Gets the fixture mappings that were created during <see cref="AfterTestClassStartingAsync"/>.
        /// </summary>
        protected Dictionary<Type, object> ClassFixtureMappings { get; set; }

        void CreateFixture(Type fixtureGenericInterfaceType)
        {
            var fixtureType = fixtureGenericInterfaceType.GetTypeInfo().GenericTypeArguments.Single();
            Aggregator.Run(() => ClassFixtureMappings[fixtureType] = Activator.CreateInstance(fixtureType));
        }

        /// <inheritdoc/>
        protected override string FormatConstructorArgsMissingMessage(ConstructorInfo constructor, IReadOnlyList<Tuple<int, ParameterInfo>> unusedArguments)
        {
            var argText = String.Join(", ", unusedArguments.Select(arg => String.Format("{0} {1}", arg.Item2.ParameterType.Name, arg.Item2.Name)));
            return String.Format("The following constructor parameters did not have matching fixture data: {0}", argText);
        }

        /// <inheritdoc/>
        protected override Task AfterTestClassStartingAsync()
        {
            var ordererAttribute = Class.GetCustomAttributes(typeof(TestCaseOrdererAttribute)).SingleOrDefault();
            if (ordererAttribute != null)
                TestCaseOrderer = ExtensibilityPointFactory.GetTestCaseOrderer(diagnosticMessageSink, ordererAttribute);

            var testClassTypeInfo = Class.Type.GetTypeInfo();
            if (testClassTypeInfo.ImplementedInterfaces.Any(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollectionFixture<>)))
                Aggregator.Add(new TestClassException("A test class may not be decorated with ICollectionFixture<> (decorate the test collection class instead)."));

            foreach (var interfaceType in testClassTypeInfo.ImplementedInterfaces.Where(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IClassFixture<>)))
                CreateFixture(interfaceType);

            if (TestClass.TestCollection.CollectionDefinition != null)
            {
                var declarationType = ((IReflectionTypeInfo)TestClass.TestCollection.CollectionDefinition).Type;
                foreach (var interfaceType in declarationType.GetTypeInfo().ImplementedInterfaces.Where(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IClassFixture<>)))
                    CreateFixture(interfaceType);
            }

            return Task.FromResult(0);
        }

        /// <inheritdoc/>
        protected override Task BeforeTestClassFinishedAsync()
        {
            foreach (var fixture in ClassFixtureMappings.Values.OfType<IDisposable>())
                Aggregator.Run(fixture.Dispose);

            return Task.FromResult(0);
        }

        /// <inheritdoc/>
        protected override Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, object[] constructorArguments)
        {
            return new XunitTestMethodRunner(testMethod, Class, method, testCases, diagnosticMessageSink, MessageBus, new ExceptionAggregator(Aggregator), CancellationTokenSource, constructorArguments).RunAsync();
        }

        /// <inheritdoc/>
        protected override ConstructorInfo SelectTestClassConstructor()
        {
            var ctors = Class.Type.GetTypeInfo()
                                  .DeclaredConstructors
                                  .Where(ci => !ci.IsStatic && ci.IsPublic)
                                  .ToList();

            if (ctors.Count == 1)
                return ctors[0];

            Aggregator.Add(new TestClassException("A test class may only define a single public constructor."));
            return null;
        }

        /// <inheritdoc/>
        protected override bool TryGetConstructorArgument(ConstructorInfo constructor, int index, ParameterInfo parameter, out object argumentValue)
        {
            if (parameter.ParameterType == typeof(ITestOutputHelper))
            {
                argumentValue = new TestOutputHelper();
                return true;
            }

            return ClassFixtureMappings.TryGetValue(parameter.ParameterType, out argumentValue)
                || collectionFixtureMappings.TryGetValue(parameter.ParameterType, out argumentValue);
        }
    }
}
