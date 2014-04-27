using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xunit.Abstractions;
using System.Threading.Tasks;

namespace Xunit.Sdk
{
    public class XunitTestClassRunner : TestClassRunner<IXunitTestCase>
    {
        readonly IDictionary<Type, object> collectionFixtureMappings;
        readonly IDictionary<Type, object> fixtureMappings = new Dictionary<Type, object>();

        public XunitTestClassRunner(IMessageBus messageBus,
                                    ITestCollection testCollection,
                                    IReflectionTypeInfo testClass,
                                    IEnumerable<IXunitTestCase> testCases,
                                    ITestCaseOrderer testCaseOrderer,
                                    ExceptionAggregator aggregator,
                                    CancellationTokenSource cancellationTokenSource,
                                    IDictionary<Type, object> collectionFixtureMappings)
            : base(messageBus, testCollection, testClass, testCases, testCaseOrderer, aggregator, cancellationTokenSource)
        {
            this.collectionFixtureMappings = collectionFixtureMappings;
        }

        void CreateFixture(Type fixtureGenericInterfaceType)
        {
            var fixtureType = fixtureGenericInterfaceType.GetGenericArguments().Single();
            Aggregator.Run(() => fixtureMappings[fixtureType] = Activator.CreateInstance(fixtureType));
        }

        static ITestCaseOrderer GetXunitTestCaseOrderer(IAttributeInfo ordererAttribute)
        {
            var args = ordererAttribute.GetConstructorArguments().Cast<string>().ToList();
            var ordererType = Reflector.GetType(args[1], args[0]);
            return ExtensibilityPointFactory.GetTestCaseOrderer(ordererType);
        }

        protected override void OnTestClassStarting()
        {
            var ordererAttribute = TestClass.GetCustomAttributes(typeof(TestCaseOrdererAttribute)).SingleOrDefault();
            if (ordererAttribute != null)
                TestCaseOrderer = GetXunitTestCaseOrderer(ordererAttribute);

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

        protected override Task<RunSummary> RunTestMethodAsync(object[] constructorArguments, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases)
        {
            var testMethodRunner = new XunitTestMethodRunner(MessageBus, TestCollection, TestClass, method, testCases, CancellationTokenSource, Aggregator, constructorArguments);
            return testMethodRunner.RunTestMethodAsync();
        }

        protected override ConstructorInfo SelectTestClassConstructor()
        {
            var ctors = TestClass.Type.GetConstructors();
            if (ctors.Length == 1)
                return ctors[0];

            Aggregator.Add(new TestClassException("A test class may only define a single public constructor."));
            return null;
        }

        protected override bool TryGetConstructorArgument(ConstructorInfo constructor, int index, ParameterInfo parameter, out object argumentValue)
        {
            return fixtureMappings.TryGetValue(parameter.ParameterType, out argumentValue)
                || collectionFixtureMappings.TryGetValue(parameter.ParameterType, out argumentValue);
        }
    }
}
