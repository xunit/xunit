﻿using System;
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
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="testCaseOrderer">The test case orderer that will be used to decide how to order the test.</param>
        /// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        public XunitTestCollectionRunner(ITestCollection testCollection,
                                         IEnumerable<IXunitTestCase> testCases,
                                         IMessageBus messageBus,
                                         ITestCaseOrderer testCaseOrderer,
                                         ExceptionAggregator aggregator,
                                         CancellationTokenSource cancellationTokenSource)
            : base(testCollection, testCases, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
        {
            CollectionFixtureMappings = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Gets the fixture mappings that were created during <see cref="AfterTestCollectionStarting"/>.
        /// </summary>
        protected Dictionary<Type, object> CollectionFixtureMappings { get; set; }

        void CreateFixture(Type fixtureGenericInterfaceType)
        {
            var fixtureType = fixtureGenericInterfaceType.GenericTypeArguments.Single();
            Aggregator.Run(() => CollectionFixtureMappings[fixtureType] = Activator.CreateInstance(fixtureType));
        }

        /// <inheritdoc/>
        protected override void AfterTestCollectionStarting()
        {
            if (TestCollection.CollectionDefinition != null)
            {
                var declarationType = ((IReflectionTypeInfo)TestCollection.CollectionDefinition).Type;
                foreach (var interfaceType in declarationType.GetTypeInfo().ImplementedInterfaces.Where(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollectionFixture<>)))
                    CreateFixture(interfaceType);

                var ordererAttribute = TestCollection.CollectionDefinition.GetCustomAttributes(typeof(TestCaseOrdererAttribute)).SingleOrDefault();
                if (ordererAttribute != null)
                    TestCaseOrderer = ExtensibilityPointFactory.GetTestCaseOrderer(ordererAttribute);
            }
        }

        /// <inheritdoc/>
        protected override void BeforeTestCollectionFinished()
        {
            foreach (var fixture in CollectionFixtureMappings.Values.OfType<IDisposable>())
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
        protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases)
        {
            return new XunitTestClassRunner(testClass, @class, testCases, MessageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), CancellationTokenSource, CollectionFixtureMappings).RunAsync();
        }
    }
}
