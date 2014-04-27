using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public abstract class TestClassRunner<TTestCase>
        where TTestCase : ITestCase
    {
        public TestClassRunner(IMessageBus messageBus,
                               ITestCollection testCollection,
                               IReflectionTypeInfo testClass,
                               IEnumerable<TTestCase> testCases,
                               ITestCaseOrderer testCaseOrderer,
                               ExceptionAggregator aggregator,
                               CancellationTokenSource cancellationTokenSource)
        {
            MessageBus = messageBus;
            TestCollection = testCollection;
            TestClass = testClass;
            TestCases = testCases;
            TestCaseOrderer = testCaseOrderer;
            Aggregator = aggregator;
            CancellationTokenSource = cancellationTokenSource;
        }

        protected ExceptionAggregator Aggregator { get; set; }

        protected CancellationTokenSource CancellationTokenSource { get; set; }

        protected IMessageBus MessageBus { get; set; }

        protected ITestCaseOrderer TestCaseOrderer { get; set; }

        protected IEnumerable<TTestCase> TestCases { get; set; }

        protected IReflectionTypeInfo TestClass { get; set; }

        protected ITestCollection TestCollection { get; set; }

        protected virtual object[] CreateTestClassConstructorArguments()
        {
            var constructorArguments = new List<object>();

            var isStaticClass = TestClass.Type.IsAbstract && TestClass.Type.IsSealed;
            if (!isStaticClass)
            {

                var ctor = SelectTestClassConstructor();
                if (ctor != null)
                {
                    var unusedArguments = new List<string>();
                    var parameters = ctor.GetParameters();

                    for (int idx = 0; idx < parameters.Length; ++idx)
                    {
                        var parameter = parameters[idx];
                        object fixture;

                        if (TryGetConstructorArgument(ctor, idx, parameter, out fixture))
                            constructorArguments.Add(fixture);
                        else
                            unusedArguments.Add(String.Format("{0} {1}", parameter.ParameterType.Name, parameter.Name));
                    }

                    if (unusedArguments.Count > 0)
                        Aggregator.Add(new TestClassException("The following constructor arguments did not have matching fixture data: " + String.Join(", ", unusedArguments)));
                }
            }

            return constructorArguments.ToArray();
        }

        protected virtual void OnTestClassStarting() { }

        protected virtual void OnTestClassFinished() { }

        public async Task<RunSummary> RunAsync()
        {
            OnTestClassStarting();

            var classSummary = new RunSummary();

            if (!MessageBus.QueueMessage(new TestClassStarting(TestCollection, TestClass.Name)))
                CancellationTokenSource.Cancel();
            else
            {
                var orderedTestCases = TestCaseOrderer.OrderTestCases(TestCases);
                var methodGroups = orderedTestCases.GroupBy(tc => tc.Method);
                var constructorArguments = CreateTestClassConstructorArguments();

                foreach (var method in methodGroups)
                {
                    var methodSummary = await RunTestMethodAsync(constructorArguments, (IReflectionMethodInfo)method.Key, method);
                    classSummary.Aggregate(methodSummary);

                    if (CancellationTokenSource.IsCancellationRequested)
                        break;
                }
            }

            if (!MessageBus.QueueMessage(new TestClassFinished(TestCollection, TestClass.Name, classSummary.Time, classSummary.Total, classSummary.Failed, classSummary.Skipped)))
                CancellationTokenSource.Cancel();

            OnTestClassFinished();

            return classSummary;
        }

        protected abstract Task<RunSummary> RunTestMethodAsync(object[] constructorArguments, IReflectionMethodInfo method, IEnumerable<TTestCase> testCases);

        protected virtual ConstructorInfo SelectTestClassConstructor()
        {
            var result = TestClass.Type.GetConstructor(new Type[0]);
            if (result == null)
                Aggregator.Add(new TestClassException("A test class must have a parameterless constructor."));

            return result;
        }

        protected virtual bool TryGetConstructorArgument(ConstructorInfo constructor, int index, ParameterInfo parameter, out object argumentValue)
        {
            argumentValue = null;
            return false;
        }
    }
}
