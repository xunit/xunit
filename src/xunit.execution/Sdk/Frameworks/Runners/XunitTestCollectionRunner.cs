using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The test collection runner for xUnit.net v2 tests.
    /// </summary>
    public class XunitTestCollectionRunner : TestCollectionRunner<IXunitTestCase>
    {
        readonly Dictionary<Type, object> collectionFixtureMappings = new Dictionary<Type, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestCollectionRunner"/> class.
        /// </summary>
        /// <param name="testCollection">The test collection that contains the tests to be run.</param>
        /// <param name="testCases">The test cases to be run.</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="testCaseOrderer">The test case orderer that will be used to decide how to order the test.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        public XunitTestCollectionRunner(ITestCollection testCollection,
                                         IEnumerable<IXunitTestCase> testCases,
                                         IMessageBus messageBus,
                                         ITestCaseOrderer testCaseOrderer,
                                         CancellationTokenSource cancellationTokenSource)
            : base(testCollection, testCases, messageBus, testCaseOrderer, cancellationTokenSource) { }

        void CreateFixture(Type fixtureGenericInterfaceType)
        {
            var fixtureType = fixtureGenericInterfaceType.GetGenericArguments().Single();
            Aggregator.Run(() => collectionFixtureMappings[fixtureType] = Activator.CreateInstance(fixtureType));
        }

        /// <inheritdoc/>
        protected override void OnTestCollectionStarting()
        {
            if (TestCollection.CollectionDefinition != null)
            {
                var declarationType = ((IReflectionTypeInfo)TestCollection.CollectionDefinition).Type;
                foreach (var interfaceType in declarationType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollectionFixture<>)))
                    CreateFixture(interfaceType);

                var ordererAttribute = TestCollection.CollectionDefinition.GetCustomAttributes(typeof(TestCaseOrdererAttribute)).SingleOrDefault();
                if (ordererAttribute != null)
                    TestCaseOrderer = ExtensibilityPointFactory.GetTestCaseOrderer(ordererAttribute);
            }
        }

        /// <inheritdoc/>
        protected override void OnTestCollectionFinishing()
        {
            foreach (var fixture in collectionFixtureMappings.Values.OfType<IDisposable>())
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
        protected override Task<RunSummary> RunTestClassAsync(IReflectionTypeInfo testClass, IEnumerable<IXunitTestCase> testCases)
        {
            var testClassRunner = new XunitTestClassRunner(TestCollection, testClass, testCases, MessageBus, TestCaseOrderer, Aggregator, CancellationTokenSource, collectionFixtureMappings);
            return testClassRunner.RunAsync();
        }
    }
}
