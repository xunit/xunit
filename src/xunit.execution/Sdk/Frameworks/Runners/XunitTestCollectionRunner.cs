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
    /// The test collection runner for xUnit.net v2 tests.
    /// </summary>
    public class XunitTestCollectionRunner : TestCollectionRunner<IXunitTestCase>
    {
        readonly IMessageSink diagnosticMessageSink;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestCollectionRunner"/> class.
        /// </summary>
        /// <param name="testCollection">The test collection that contains the tests to be run.</param>
        /// <param name="testCases">The test cases to be run.</param>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="testCaseOrderer">The test case orderer that will be used to decide how to order the test.</param>
        /// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        public XunitTestCollectionRunner(ITestCollection testCollection,
                                         IEnumerable<IXunitTestCase> testCases,
                                         IMessageSink diagnosticMessageSink,
                                         IMessageBus messageBus,
                                         ITestCaseOrderer testCaseOrderer,
                                         ExceptionAggregator aggregator,
                                         CancellationTokenSource cancellationTokenSource)
            : base(testCollection, testCases, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        /// <summary>
        /// Gets the fixture mappings that were created during <see cref="AfterTestCollectionStartingAsync"/>.
        /// </summary>
        protected Dictionary<Type, object> CollectionFixtureMappings { get; set; } = new Dictionary<Type, object>();

        /// <inheritdoc/>
        protected override Task AfterTestCollectionStartingAsync()
        {
            CreateCollectionFixtures();
            TestCaseOrderer = GetTestCaseOrderer() ?? TestCaseOrderer;

            return CommonTasks.Completed;
        }

        /// <inheritdoc/>
        protected override Task BeforeTestCollectionFinishedAsync()
        {
            foreach (var fixture in CollectionFixtureMappings.Values.OfType<IDisposable>())
                Aggregator.Run(fixture.Dispose);

            return CommonTasks.Completed;
        }

        /// <summary>
        /// Creates the instance of a collection fixture type to be used by the test collection. If the fixture can be created,
        /// it should be placed into the <see cref="CollectionFixtureMappings"/> dictionary; if it cannot, then the method
        /// should record the error by calling <code>Aggregator.Add</code>.
        /// </summary>
        /// <param name="fixtureType">The type of the fixture to be created</param>
        protected virtual void CreateCollectionFixture(Type fixtureType)
            => Aggregator.Run(() => CollectionFixtureMappings[fixtureType] = Activator.CreateInstance(fixtureType));

        void CreateCollectionFixtures()
        {
            if (TestCollection.CollectionDefinition != null)
            {
                var declarationType = ((IReflectionTypeInfo)TestCollection.CollectionDefinition).Type;
                foreach (var interfaceType in declarationType.GetTypeInfo().ImplementedInterfaces.Where(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollectionFixture<>)))
                    CreateCollectionFixture(interfaceType.GenericTypeArguments.Single());
            }
        }

        /// <summary>
        /// Gives an opportunity to override test case orderer. By default, this method gets the
        /// orderer from the collection definition. If this function returns <c>null</c>, the
        /// test case orderer passed into the constructor will be used.
        /// </summary>
        protected virtual ITestCaseOrderer GetTestCaseOrderer()
        {
            if (TestCollection.CollectionDefinition != null)
            {
                var ordererAttribute = TestCollection.CollectionDefinition.GetCustomAttributes(typeof(TestCaseOrdererAttribute)).SingleOrDefault();
                if (ordererAttribute != null)
                {
                    try
                    {
                        var testCaseOrderer = ExtensibilityPointFactory.GetTestCaseOrderer(diagnosticMessageSink, ordererAttribute);
                        if (testCaseOrderer != null)
                            return testCaseOrderer;

                        var args = ordererAttribute.GetConstructorArguments().Cast<string>().ToList();
                        diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Could not find type '{args[0]}' in {args[1]} for collection-level test case orderer on test collection '{TestCollection.DisplayName}'"));
                    }
                    catch (Exception ex)
                    {
                        var innerEx = ex.Unwrap();
                        var args = ordererAttribute.GetConstructorArguments().Cast<string>().ToList();
                        diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Collection-level test case orderer '{args[0]}' for test collection '{TestCollection.DisplayName}' threw '{innerEx.GetType().FullName}' during construction: {innerEx.Message}{Environment.NewLine}{innerEx.StackTrace}"));
                    }
                }
            }

            return null;
        }

        /// <inheritdoc/>
        protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases)
            => new XunitTestClassRunner(testClass, @class, testCases, diagnosticMessageSink, MessageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), CancellationTokenSource, CollectionFixtureMappings).RunAsync();
    }
}
