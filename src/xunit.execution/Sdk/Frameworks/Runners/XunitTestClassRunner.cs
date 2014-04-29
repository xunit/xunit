using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xunit.Abstractions;
using System.Threading.Tasks;

namespace Xunit.Sdk
{
    /// <summary>
    /// The test class runner for xUnit.net v2 tests.
    /// </summary>
    public class XunitTestClassRunner : TestClassRunner<IXunitTestCase>
    {
        readonly IDictionary<Type, object> collectionFixtureMappings;
        readonly IDictionary<Type, object> fixtureMappings = new Dictionary<Type, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestClassRunner"/> class.
        /// </summary>
        /// <param name="testCollection">The test collection that contains the test class.</param>
        /// <param name="testClass">The test class that contains the tests to be run.</param>
        /// <param name="testCases">The test cases to be run.</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="testCaseOrderer">The test case orderer that will be used to decide how to order the test.</param>
        /// <param name="aggregator">The exception aggregator used to run code and collection exceptions.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        /// <param name="collectionFixtureMappings">The mapping of collection fixture types to fixtures.</param>
        public XunitTestClassRunner(ITestCollection testCollection,
                                    IReflectionTypeInfo testClass,
                                    IEnumerable<IXunitTestCase> testCases,
                                    IMessageBus messageBus,
                                    ITestCaseOrderer testCaseOrderer,
                                    ExceptionAggregator aggregator,
                                    CancellationTokenSource cancellationTokenSource,
                                    IDictionary<Type, object> collectionFixtureMappings)
            : base(testCollection, testClass, testCases, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
        {
            this.collectionFixtureMappings = collectionFixtureMappings;
        }

        void CreateFixture(Type fixtureGenericInterfaceType)
        {
            var fixtureType = fixtureGenericInterfaceType.GetGenericArguments().Single();
            Aggregator.Run(() => fixtureMappings[fixtureType] = Activator.CreateInstance(fixtureType));
        }

        /// <inheritdoc/>
        protected override void OnTestClassStarting()
        {
            var ordererAttribute = TestClass.GetCustomAttributes(typeof(TestCaseOrdererAttribute)).SingleOrDefault();
            if (ordererAttribute != null)
                TestCaseOrderer = ExtensibilityPointFactory.GetTestCaseOrderer(ordererAttribute);

            if (TestClass.Type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollectionFixture<>)))
                Aggregator.Add(new TestClassException("A test class may not be decorated with ICollectionFixture<> (decorate the test collection class instead)."));

            foreach (var interfaceType in TestClass.Type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IClassFixture<>)))
                CreateFixture(interfaceType);

            if (TestCollection.CollectionDefinition != null)
            {
                var declarationType = ((IReflectionTypeInfo)TestCollection.CollectionDefinition).Type;
                foreach (var interfaceType in declarationType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IClassFixture<>)))
                    CreateFixture(interfaceType);
            }
        }

        /// <inheritdoc/>
        protected override void OnTestClassFinished()
        {
            foreach (var fixture in fixtureMappings.Values.OfType<IDisposable>())
            {
                try
                {
                    fixture.Dispose();
                }
                catch (Exception ex)
                {
                    if (!MessageBus.QueueMessage(new ErrorMessage(ex.Unwrap())))
                        CancellationTokenSource.Cancel();
                }
            }
        }

        /// <inheritdoc/>
        protected override Task<RunSummary> RunTestMethodAsync(IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, object[] constructorArguments)
        {
            var testMethodRunner = new XunitTestMethodRunner(TestCollection, TestClass, method, testCases, MessageBus, CancellationTokenSource, Aggregator, constructorArguments);
            return testMethodRunner.RunAsync();
        }

        /// <inheritdoc/>
        protected override ConstructorInfo SelectTestClassConstructor()
        {
            var ctors = TestClass.Type.GetConstructors();
            if (ctors.Length == 1)
                return ctors[0];

            Aggregator.Add(new TestClassException("A test class may only define a single public constructor."));
            return null;
        }

        /// <inheritdoc/>
        protected override bool TryGetConstructorArgument(ConstructorInfo constructor, int index, ParameterInfo parameter, out object argumentValue)
        {
            return fixtureMappings.TryGetValue(parameter.ParameterType, out argumentValue)
                || collectionFixtureMappings.TryGetValue(parameter.ParameterType, out argumentValue);
        }
    }
}
