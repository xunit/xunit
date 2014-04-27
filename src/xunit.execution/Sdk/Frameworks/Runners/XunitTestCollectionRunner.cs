using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit.Abstractions;
using System.Threading.Tasks;

namespace Xunit.Sdk
{
    public class XunitTestCollectionRunner : TestCollectionRunner<IXunitTestCase>
    {
        readonly Dictionary<Type, object> collectionFixtureMappings = new Dictionary<Type, object>();

        public XunitTestCollectionRunner(IMessageBus messageBus,
                                         ITestCollection testCollection,
                                         IEnumerable<IXunitTestCase> testCases,
                                         ITestCaseOrderer testCaseOrderer,
                                         CancellationTokenSource cancellationTokenSource)
            : base(messageBus, testCollection, testCases, testCaseOrderer, cancellationTokenSource)
        {
            
        }

        void CreateFixture(Type fixtureGenericInterfaceType)
        {
            var fixtureType = fixtureGenericInterfaceType.GetGenericArguments().Single();
            Aggregator.Run(() => collectionFixtureMappings[fixtureType] = Activator.CreateInstance(fixtureType));
        }

        static ITestCaseOrderer GetXunitTestCaseOrderer(IAttributeInfo ordererAttribute)
        {
            var args = ordererAttribute.GetConstructorArguments().Cast<string>().ToList();
            var ordererType = Reflector.GetType(args[1], args[0]);
            return ExtensibilityPointFactory.GetTestCaseOrderer(ordererType);
        }
        
        protected override void OnTestCollectionStarting()
        {
            if (TestCollection.CollectionDefinition != null)
            {
                var declarationType = ((IReflectionTypeInfo)TestCollection.CollectionDefinition).Type;
                foreach (var interfaceType in declarationType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollectionFixture<>)))
                    CreateFixture(interfaceType);

                var ordererAttribute = TestCollection.CollectionDefinition.GetCustomAttributes(typeof(TestCaseOrdererAttribute)).SingleOrDefault();
                if (ordererAttribute != null)
                    TestCaseOrderer = GetXunitTestCaseOrderer(ordererAttribute);
            }
        }

        protected override void OnTestCollectionFinished()
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

        protected override Task<RunSummary> RunTestClassAsync(IReflectionTypeInfo testClass, IEnumerable<IXunitTestCase> testCases)
        {
            var testClassRunner = new XunitTestClassRunner(MessageBus, TestCollection, testClass, testCases, TestCaseOrderer, Aggregator, CancellationTokenSource, collectionFixtureMappings);
            return testClassRunner.RunAsync();
        }
    }
}
