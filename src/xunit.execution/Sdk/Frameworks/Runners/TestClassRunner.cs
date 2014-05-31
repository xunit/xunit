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
    /// A base class that provides default behavior when running tests in a test class. It groups the tests
    /// by test method, and then runs the individual test methods.
    /// </summary>
    /// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
    /// derive from <see cref="ITestCase"/>.</typeparam>
    public abstract class TestClassRunner<TTestCase>
        where TTestCase : ITestCase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestClassRunner{TTestCase}"/> class.
        /// </summary>
        /// <param name="testCollection">The test collection that contains the test class.</param>
        /// <param name="testClass">The test class that contains the tests to be run.</param>
        /// <param name="testCases">The test cases to be run.</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="testCaseOrderer">The test case orderer that will be used to decide how to order the test.</param>
        /// <param name="aggregator">The exception aggregator used to run code and collection exceptions.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        public TestClassRunner(ITestCollection testCollection,
                               IReflectionTypeInfo testClass,
                               IEnumerable<TTestCase> testCases,
                               IMessageBus messageBus,
                               ITestCaseOrderer testCaseOrderer,
                               ExceptionAggregator aggregator,
                               CancellationTokenSource cancellationTokenSource)
        {
            TestCollection = testCollection;
            TestClass = testClass;
            TestCases = testCases;
            MessageBus = messageBus;
            TestCaseOrderer = testCaseOrderer;
            Aggregator = aggregator;
            CancellationTokenSource = cancellationTokenSource;
        }

        /// <summary>
        /// Gets or sets the exception aggregator used to run code and collection exceptions.
        /// </summary>
        protected ExceptionAggregator Aggregator { get; set; }

        /// <summary>
        /// Gets or sets he task cancellation token source, used to cancel the test run.
        /// </summary>
        protected CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        /// Gets or sets the message bus to report run status to.
        /// </summary>
        protected IMessageBus MessageBus { get; set; }

        /// <summary>
        /// Gets or sets the test case orderer that will be used to decide how to order the test.
        /// </summary>
        protected ITestCaseOrderer TestCaseOrderer { get; set; }

        /// <summary>
        /// Gets or sets the test cases to be run.
        /// </summary>
        protected IEnumerable<TTestCase> TestCases { get; set; }

        /// <summary>
        /// Gets or sets the test class that contains the tests to be run.
        /// </summary>
        protected IReflectionTypeInfo TestClass { get; set; }

        /// <summary>
        /// Gets or sets the test collection that contains the test class.
        /// </summary>
        protected ITestCollection TestCollection { get; set; }

        /// <summary>
        /// Creates the arguments for the test class constructor. Attempts to resolve each parameter
        /// individually, and adds an error when the constructor arguments cannot all be provided.
        /// If the class is static, does not look for constructor, since one will not be needed.
        /// </summary>
        /// <returns>The test class constructor arguments.</returns>
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

        /// <summary>
        /// This method is called just before <see cref="ITestClassStarting"/> is sent.
        /// </summary>
        protected virtual void OnTestClassStarting() { }

        /// <summary>
        /// This method is called just after <see cref="ITestClassStarting"/> is sent, but before any test methods are run.
        /// </summary>
        protected virtual void OnTestClassStarted() { }

        /// <summary>
        /// This method is called just before <see cref="ITestClassFinished"/> is sent.
        /// </summary>
        protected virtual void OnTestClassFinishing() { }

        /// <summary>
        /// This method is called just after <see cref="ITestClassFinished"/> is sent.
        /// </summary>
        protected virtual void OnTestClassFinished() { }

        /// <summary>
        /// Runs the tests in the test class.
        /// </summary>
        /// <returns>Returns summary information about the tests that were run.</returns>
        public async Task<RunSummary> RunAsync()
        {
            OnTestClassStarting();

            var classSummary = new RunSummary();

            try
            {
                if (!MessageBus.QueueMessage(new TestClassStarting(TestCollection, TestClass.Name)))
                    CancellationTokenSource.Cancel();
                else
                {
                    OnTestClassStarted();

                    classSummary = await RunTestMethodsAsync();
                }
            }
            finally
            {
                OnTestClassFinishing();

                if (!MessageBus.QueueMessage(new TestClassFinished(TestCollection, TestClass.Name, classSummary.Time, classSummary.Total, classSummary.Failed, classSummary.Skipped)))
                    CancellationTokenSource.Cancel();

                OnTestClassFinished();
            }

            return classSummary;
        }

        /// <summary>
        /// Runs the list of test methods. By default, orders the tests, groups them by method and runs them synchronously.
        /// </summary>
        /// <returns>Returns summary information about the tests that were run.</returns>
        protected virtual async Task<RunSummary> RunTestMethodsAsync()
        {
            var summary = new RunSummary();
            var orderedTestCases = TestCaseOrderer.OrderTestCases(TestCases);
            var constructorArguments = CreateTestClassConstructorArguments();

            foreach (var method in orderedTestCases.GroupBy(tc => tc.Method, MethodInfoNameEqualityComparer.Instance))
            {
                summary.Aggregate(await RunTestMethodAsync((IReflectionMethodInfo)method.Key, method, constructorArguments));
                if (CancellationTokenSource.IsCancellationRequested)
                    break;
            }

            return summary;
        }

        /// <summary>
        /// Override this method to run the tests in an individual test method.
        /// </summary>
        /// <param name="testMethod">The test method that contains the tests to be run.</param>
        /// <param name="testCases">The test cases to be run.</param>
        /// <param name="constructorArguments">The constructor arguments that will be used to create the test class.</param>
        /// <returns>Returns summary information about the tests that were run.</returns>
        protected abstract Task<RunSummary> RunTestMethodAsync(IReflectionMethodInfo testMethod, IEnumerable<TTestCase> testCases, object[] constructorArguments);

        /// <summary>
        /// Selects the constructor to be used for the test class. By default, chooses the parameterless
        /// constructor. Override to change the constructor selection logic.
        /// </summary>
        /// <returns>The constructor to be used for creating the test class.</returns>
        protected virtual ConstructorInfo SelectTestClassConstructor()
        {
            var result = TestClass.Type.GetConstructor(new Type[0]);
            if (result == null)
                Aggregator.Add(new TestClassException("A test class must have a parameterless constructor."));

            return result;
        }

        /// <summary>
        /// Tries to supply a test class constructor argument. By default, always fails. Override to
        /// change the argument lookup logic.
        /// </summary>
        /// <param name="constructor">The constructor that will be used to create the test class.</param>
        /// <param name="index">The parameter index.</param>
        /// <param name="parameter">The parameter information.</param>
        /// <param name="argumentValue">The argument value that should be used for the parameter.</param>
        /// <returns>Returns <c>true</c> if the argument was supplied; <c>false</c>, otherwise.</returns>
        protected virtual bool TryGetConstructorArgument(ConstructorInfo constructor, int index, ParameterInfo parameter, out object argumentValue)
        {
            argumentValue = null;
            return false;
        }

        private class MethodInfoNameEqualityComparer : IEqualityComparer<IMethodInfo>
        {
            public static readonly MethodInfoNameEqualityComparer Instance = new MethodInfoNameEqualityComparer();

            public bool Equals(IMethodInfo x, IMethodInfo y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(IMethodInfo obj)
            {
                return obj.Name.GetHashCode();
            }
        }
    }
}
