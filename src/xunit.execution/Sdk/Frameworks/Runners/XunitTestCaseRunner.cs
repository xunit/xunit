using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.Sdk
{
    /// <summary>
    /// The test case runner for xUnit.net v2 tests.
    /// </summary>
    public class XunitTestCaseRunner : TestCaseRunner<IXunitTestCase>
    {
        readonly List<BeforeAfterTestAttribute> beforeAfterAttributes;

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

            ParameterInfo[] parameters = TestMethod.GetParameters();
            Type[] parameterTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                parameterTypes[i] = parameters[i].ParameterType;

            TestMethodArguments = Reflector.ConvertArguments(testMethodArguments, parameterTypes);

            beforeAfterAttributes =
                TestClass.GetTypeInfo().GetCustomAttributes(typeof(BeforeAfterTestAttribute))
                         .Concat(TestMethod.GetCustomAttributes(typeof(BeforeAfterTestAttribute)))
                         .Cast<BeforeAfterTestAttribute>()
                         .ToList();
        }

        /// <summary>
        /// Gets the list of <see cref="BeforeAfterTestAttribute"/>s that will be used for this test case.
        /// </summary>
        public IReadOnlyList<BeforeAfterTestAttribute> BeforeAfterAttributes
            => beforeAfterAttributes;

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

        /// <inheritdoc/>
        protected override Task<RunSummary> RunTestAsync()
            => new XunitTestRunner(new XunitTest(TestCase, DisplayName), MessageBus, TestClass, ConstructorArguments, TestMethod, TestMethodArguments, SkipReason, beforeAfterAttributes, new ExceptionAggregator(Aggregator), CancellationTokenSource).RunAsync();
    }
}
