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
    /// The test case runner for xUnit.net v2 tests.
    /// </summary>
    public class XunitTestCaseRunner : TestCaseRunner<IXunitTestCase>
    {
        List<BeforeAfterTestAttribute> beforeAfterAttributes;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestCaseRunner"/> class.
        /// </summary>
        /// <param name="testCase">The test case to be run.</param>
        /// <param name="displayName">The display name of the test case.</param>
        /// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
        /// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
        /// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        public XunitTestCaseRunner(IXunitTestCase testCase,
                                   string displayName,
                                   string skipReason,
                                   object[] constructorArguments,
                                   object[] testMethodArguments,
                                   IMessageBus messageBus,
                                   ExceptionAggregator aggregator,
                                   CancellationTokenSource cancellationTokenSource)
            : base(testCase, messageBus, aggregator, cancellationTokenSource)
        {
            DisplayName = displayName;
            SkipReason = skipReason;
            ConstructorArguments = constructorArguments;

            TestClass = TestCase.TestMethod.TestClass.Class.ToRuntimeType();
            TestMethod = TestCase.Method.ToRuntimeMethod();

            var parameters = TestMethod.GetParameters();
            var parameterTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                parameterTypes[i] = parameters[i].ParameterType;

            TestMethodArguments = Reflector.ConvertArguments(testMethodArguments, parameterTypes);
        }

        /// <summary>
        /// Gets the list of <see cref="BeforeAfterTestAttribute"/>s that will be used for this test case.
        /// </summary>
        public IReadOnlyList<BeforeAfterTestAttribute> BeforeAfterAttributes
        {
            get
            {
                if (beforeAfterAttributes == null)
                    beforeAfterAttributes = GetBeforeAfterTestAttributes();

                return beforeAfterAttributes;
            }
        }

        /// <summary>
        /// Gets or sets the arguments passed to the test class constructor
        /// </summary>
        protected object[] ConstructorArguments { get; set; }

        /// <summary>
        /// Gets or sets the display name of the test case
        /// </summary>
        protected string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the skip reason for the test, if set.
        /// </summary>
        protected string SkipReason { get; set; }

        /// <summary>
        /// Gets or sets the runtime type for the test class that the test method belongs to.
        /// </summary>
        protected Type TestClass { get; set; }

        /// <summary>
        /// Gets of sets the runtime method for the test method that the test case belongs to.
        /// </summary>
        protected MethodInfo TestMethod { get; set; }

        /// <summary>
        /// Gets or sets the arguments to pass to the test method when it's being invoked.
        /// </summary>
        protected object[] TestMethodArguments { get; set; }

        /// <summary>
        /// Creates the <see cref="ITest"/> instance for the given test case.
        /// </summary>
        protected virtual ITest CreateTest(IXunitTestCase testCase, string displayName)
            => new XunitTest(testCase, displayName);

        /// <summary>
        /// Creates the test runner used to run the given test.
        /// </summary>
        protected virtual XunitTestRunner CreateTestRunner(ITest test,
                                                           IMessageBus messageBus,
                                                           Type testClass,
                                                           object[] constructorArguments,
                                                           MethodInfo testMethod,
                                                           object[] testMethodArguments,
                                                           string skipReason,
                                                           IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
                                                           ExceptionAggregator aggregator,
                                                           CancellationTokenSource cancellationTokenSource)
            => new XunitTestRunner(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments,
                                   skipReason, beforeAfterAttributes, new ExceptionAggregator(aggregator), cancellationTokenSource);

        /// <summary>
        /// Gets the list of <see cref="BeforeAfterTestAttribute"/> attributes that apply to this test case.
        /// </summary>
        protected virtual List<BeforeAfterTestAttribute> GetBeforeAfterTestAttributes()
        {
            IEnumerable<Attribute> beforeAfterTestCollectionAttributes;
            if (TestCase.TestMethod.TestClass.TestCollection.CollectionDefinition is IReflectionTypeInfo collectionDefinition)
                beforeAfterTestCollectionAttributes = collectionDefinition.Type.GetTypeInfo().GetCustomAttributes(typeof(BeforeAfterTestAttribute));
            else
                beforeAfterTestCollectionAttributes = Enumerable.Empty<Attribute>();

            return beforeAfterTestCollectionAttributes.Concat(TestClass.GetTypeInfo().GetCustomAttributes(typeof(BeforeAfterTestAttribute)))
                                                      .Concat(TestMethod.GetCustomAttributes(typeof(BeforeAfterTestAttribute)))
                                                      .Cast<BeforeAfterTestAttribute>()
                                                      .ToList();
        }

        /// <inheritdoc/>
        protected override Task<RunSummary> RunTestAsync()
            => CreateTestRunner(CreateTest(TestCase, DisplayName), MessageBus, TestClass, ConstructorArguments, TestMethod, TestMethodArguments,
                                SkipReason, BeforeAfterAttributes, Aggregator, CancellationTokenSource).RunAsync();
    }
}
