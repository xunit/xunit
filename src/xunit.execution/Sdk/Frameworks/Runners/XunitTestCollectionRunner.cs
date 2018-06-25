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
            this.DiagnosticMessageSink = diagnosticMessageSink;
        }

        /// <summary>
        /// Gets the fixture mappings that were created during <see cref="AfterTestCollectionStartingAsync"/>.
        /// </summary>
        protected Dictionary<Type, object> CollectionFixtureMappings { get; set; } = new Dictionary<Type, object>();

        /// <summary>
        /// Gets the message sink used to send diagnostic messages.
        /// </summary>
        protected IMessageSink DiagnosticMessageSink { get; private set; }

        /// <inheritdoc/>
        protected override async Task AfterTestCollectionStartingAsync()
        {
            await CreateCollectionFixturesAsync();
            TestCaseOrderer = GetTestCaseOrderer() ?? TestCaseOrderer;
        }

        /// <inheritdoc/>
        protected override async Task BeforeTestCollectionFinishedAsync()
        {
            var disposeAsyncTasks = CollectionFixtureMappings.Values.OfType<IAsyncLifetime>().Select(fixture => Aggregator.RunAsync(fixture.DisposeAsync)).ToList();

            await Task.WhenAll(disposeAsyncTasks);

            foreach (var fixture in CollectionFixtureMappings.Values.OfType<IDisposable>())
                Aggregator.Run(fixture.Dispose);
        }

        /// <summary>
        /// Creates the instance of a collection fixture type to be used by the test collection. If the fixture can be created,
        /// it should be placed into the <see cref="CollectionFixtureMappings"/> dictionary; if it cannot, then the method
        /// should record the error by calling <code>Aggregator.Add</code>.
        /// </summary>
        /// <param name="fixtureType">The type of the fixture to be created</param>
        protected virtual void CreateCollectionFixture(Type fixtureType)
        {
            var ctors = fixtureType.GetTypeInfo()
                .DeclaredConstructors
                .Where(ci => !ci.IsStatic && ci.IsPublic)
                .ToList();

            if (ctors.Count != 1)
            {
                Aggregator.Add(new TestClassException($"Collection fixture type '{fixtureType.FullName}' may only define a single public constructor."));
                return;
            }

            var ctor = ctors[0];
            var missingParameters = new List<ParameterInfo>();
            var ctorArgs = ctor.GetParameters().Select(p =>
            {
                object arg = null;
                if (p.ParameterType == typeof(IMessageSink))
                    arg = DiagnosticMessageSink;
                else
                    missingParameters.Add(p);
                return arg;
            }).ToArray();

            if (missingParameters.Count > 0)
                Aggregator.Add(new TestClassException(
                    $"Collection fixture type '{fixtureType.FullName}' had one or more unresolved constructor arguments: {string.Join(", ", missingParameters.Select(p => $"{p.ParameterType.Name} {p.Name}"))}"
                ));
            else
            {
                Aggregator.Run(() => CollectionFixtureMappings[fixtureType] = ctor.Invoke(ctorArgs));
            }
        }

        async Task CreateCollectionFixturesAsync()
        {
            if (TestCollection.CollectionDefinition != null)
            {
                var declarationType = ((IReflectionTypeInfo)TestCollection.CollectionDefinition).Type;
                foreach (var interfaceType in declarationType.GetTypeInfo().ImplementedInterfaces.Where(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollectionFixture<>)))
                {
                    var fixtureType = interfaceType.GenericTypeArguments.Single();
                    CreateCollectionFixture(fixtureType);
                }

                var initializeAsyncTasks = CollectionFixtureMappings.Values.OfType<IAsyncLifetime>().Select(fixture => Aggregator.RunAsync(fixture.InitializeAsync)).ToList();
                await Task.WhenAll(initializeAsyncTasks);
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
                        var testCaseOrderer = ExtensibilityPointFactory.GetTestCaseOrderer(DiagnosticMessageSink, ordererAttribute);
                        if (testCaseOrderer != null)
                            return testCaseOrderer;

                        var args = ordererAttribute.GetConstructorArguments().Cast<string>().ToList();
                        DiagnosticMessageSink.OnMessage(new DiagnosticMessage($"Could not find type '{args[0]}' in {args[1]} for collection-level test case orderer on test collection '{TestCollection.DisplayName}'"));
                    }
                    catch (Exception ex)
                    {
                        var innerEx = ex.Unwrap();
                        var args = ordererAttribute.GetConstructorArguments().Cast<string>().ToList();
                        DiagnosticMessageSink.OnMessage(new DiagnosticMessage($"Collection-level test case orderer '{args[0]}' for test collection '{TestCollection.DisplayName}' threw '{innerEx.GetType().FullName}' during construction: {innerEx.Message}{Environment.NewLine}{innerEx.StackTrace}"));
                    }
                }
            }

            return null;
        }

        /// <inheritdoc/>
        protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases)
            => new XunitTestClassRunner(testClass, @class, testCases, DiagnosticMessageSink, MessageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), CancellationTokenSource, CollectionFixtureMappings).RunAsync();
    }
}
