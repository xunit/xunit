using System;
using System.Collections.Generic;
using System.Globalization;
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
            : base(testClass, @class, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
        {
            this.collectionFixtureMappings = collectionFixtureMappings;
        }

        /// <summary>
        /// Gets the fixture mappings that were created during <see cref="AfterTestClassStartingAsync"/>.
        /// </summary>
        protected Dictionary<Type, object> ClassFixtureMappings { get; set; } = new Dictionary<Type, object>();

        /// <summary>
        /// Gets the already initialized async fixtures <see cref="CreateClassFixtureAsync"/>.
        /// </summary>
        protected HashSet<IAsyncLifetime> InitializedAsyncFixtures { get; set; } = new HashSet<IAsyncLifetime>();

        /// <summary>
        /// Creates the instance of a class fixture type to be used by the test class. If the fixture can be created,
        /// it should be placed into the <see cref="ClassFixtureMappings"/> dictionary; if it cannot, then the method
        /// should record the error by calling <code>Aggregator.Add</code>.
        /// </summary>
        /// <param name="fixtureType">The type of the fixture to be created</param>
        protected virtual void CreateClassFixture(Type fixtureType)
        {
            var ctors = fixtureType.GetTypeInfo()
                                   .DeclaredConstructors
                                   .Where(ci => !ci.IsStatic && ci.IsPublic)
                                   .ToList();

            if (ctors.Count != 1)
            {
                Aggregator.Add(new TestClassException(string.Format(CultureInfo.CurrentCulture, "Class fixture type '{0}' may only define a single public constructor.", fixtureType.FullName)));
                return;
            }

            var ctor = ctors[0];
            var missingParameters = new List<ParameterInfo>();
            var ctorArgs = ctor.GetParameters().Select(p =>
            {
                object arg;
                if (p.ParameterType == typeof(IMessageSink))
                    arg = DiagnosticMessageSink;
                else
                if (!collectionFixtureMappings.TryGetValue(p.ParameterType, out arg))
                    missingParameters.Add(p);
                return arg;
            }).ToArray();

            if (missingParameters.Count > 0)
                Aggregator.Add(
                    new TestClassException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Class fixture type '{0}' had one or more unresolved constructor arguments: {1}",
                            fixtureType.FullName,
                            string.Join(", ", missingParameters.Select(p => string.Format(CultureInfo.CurrentCulture, "{0} {1}", p.ParameterType.Name, p.Name)))
                        )
                    )
                );
            else
            {
                try
                {
                    ClassFixtureMappings[fixtureType] = ctor.Invoke(ctorArgs);
                }
                catch (Exception ex)
                {
                    Aggregator.Add(new TestClassException(string.Format(CultureInfo.CurrentCulture, "Class fixture type '{0}' threw in its constructor", fixtureType.FullName), ex.Unwrap()));
                }
            }
        }

        async Task CreateClassFixtureAsync(Type fixtureType)
        {
            CreateClassFixture(fixtureType);
            var uninitializedFixtures = ClassFixtureMappings.Values
                                        .OfType<IAsyncLifetime>()
                                        .Where(fixture => !InitializedAsyncFixtures.Contains(fixture))
                                        .ToList();

            InitializedAsyncFixtures.UnionWith(uninitializedFixtures);
            await Task.WhenAll(uninitializedFixtures.Select(fixture => Aggregator.RunAsync(fixture.InitializeAsync)));
        }

        /// <inheritdoc/>
        protected override string FormatConstructorArgsMissingMessage(ConstructorInfo constructor, IReadOnlyList<Tuple<int, ParameterInfo>> unusedArguments)
            => string.Format(
                CultureInfo.CurrentCulture,
                "The following constructor parameters did not have matching fixture data: {0}",
                string.Join(", ", unusedArguments.Select(arg => string.Format(CultureInfo.CurrentCulture, "{0} {1}", arg.Item2.ParameterType.Name, arg.Item2.Name)))
            );

        /// <inheritdoc/>
        protected override async Task AfterTestClassStartingAsync()
        {
            var ordererAttribute = Class.GetCustomAttributes(typeof(TestCaseOrdererAttribute)).SingleOrDefault();
            if (ordererAttribute != null)
            {
                try
                {
                    var testCaseOrderer = ExtensibilityPointFactory.GetTestCaseOrderer(DiagnosticMessageSink, ordererAttribute);
                    if (testCaseOrderer != null)
                        TestCaseOrderer = testCaseOrderer;
                    else
                    {
                        var args = ordererAttribute.GetConstructorArguments().Cast<string>().ToList();

                        DiagnosticMessageSink.OnMessage(
                            new DiagnosticMessage(
                                "Could not find type '{0}' in {1} for class-level test case orderer on test class '{2}'",
                                args[0],
                                args[1],
                                TestClass.Class.Name
                            )
                        );
                    }
                }
                catch (Exception ex)
                {
                    var innerEx = ex.Unwrap();
                    var args = ordererAttribute.GetConstructorArguments().Cast<string>().ToList();

                    DiagnosticMessageSink.OnMessage(
                        new DiagnosticMessage(
                            "Class-level test case orderer '{0}' for test class '{1}' threw '{2}' during construction: {3}{4}{5}",
                            args[0],
                            TestClass.Class.Name,
                            innerEx.GetType().FullName,
                            innerEx.Message,
                            Environment.NewLine,
                            innerEx.StackTrace
                        )
                    );
                }
            }

            var testClassTypeInfo = Class.Type.GetTypeInfo();
            if (testClassTypeInfo.ImplementedInterfaces.Any(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollectionFixture<>)))
                Aggregator.Add(new TestClassException("A test class may not be decorated with ICollectionFixture<> (decorate the test collection class instead)."));

            var createClassFixtureAsyncTasks = new List<Task>();
            foreach (var interfaceType in testClassTypeInfo.ImplementedInterfaces.Where(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IClassFixture<>)))
                createClassFixtureAsyncTasks.Add(CreateClassFixtureAsync(interfaceType.GetTypeInfo().GenericTypeArguments.Single()));

            if (TestClass.TestCollection.CollectionDefinition != null)
            {
                var declarationType = ((IReflectionTypeInfo)TestClass.TestCollection.CollectionDefinition).Type;
                foreach (var interfaceType in declarationType.GetTypeInfo().ImplementedInterfaces.Where(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IClassFixture<>)))
                    createClassFixtureAsyncTasks.Add(CreateClassFixtureAsync(interfaceType.GetTypeInfo().GenericTypeArguments.Single()));
            }

            await Task.WhenAll(createClassFixtureAsyncTasks);
        }

        /// <inheritdoc/>
        protected override async Task BeforeTestClassFinishedAsync()
        {
            var disposeAsyncTasks = ClassFixtureMappings.Values.OfType<IAsyncLifetime>().Select(fixture => Aggregator.RunAsync(fixture.DisposeAsync)).ToList();

            await Task.WhenAll(disposeAsyncTasks);

            foreach (var fixture in ClassFixtureMappings.Values.OfType<IDisposable>())
                Aggregator.Run(fixture.Dispose);
        }

        /// <inheritdoc/>
        protected override Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, object[] constructorArguments)
            => new XunitTestMethodRunner(testMethod, Class, method, testCases, DiagnosticMessageSink, MessageBus, new ExceptionAggregator(Aggregator), CancellationTokenSource, constructorArguments).RunAsync();

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
                // To solve https://github.com/xunit/xunit/issues/2377 we're putting a placeholder Func here,
                // and hardcoding the resolution of that in XunitTestRunner.cs. In v3, we generally support
                // using Funcs, because we resolve this via the TestContext (so it's already fixed there).
                argumentValue = () => new TestOutputHelper();
                return true;
            }

            return ClassFixtureMappings.TryGetValue(parameter.ParameterType, out argumentValue)
                || collectionFixtureMappings.TryGetValue(parameter.ParameterType, out argumentValue);
        }
    }
}
