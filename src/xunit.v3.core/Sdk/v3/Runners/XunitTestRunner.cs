using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// The test runner for xUnit.net v3 tests.
	/// </summary>
	public class XunitTestRunner : TestRunner<IXunitTestCase>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTestRunner"/> class.
		/// </summary>
		/// <param name="test">The test that this invocation belongs to.</param>
		/// <param name="messageBus">The message bus to report run status to.</param>
		/// <param name="testClass">The test class that the test method belongs to.</param>
		/// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
		/// <param name="testMethod">The test method that will be invoked.</param>
		/// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
		/// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
		/// <param name="beforeAfterAttributes">The list of <see cref="BeforeAfterTestAttribute"/>s for this test.</param>
		/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
		/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
		public XunitTestRunner(
			_ITest test,
			IMessageBus messageBus,
			Type testClass,
			object?[] constructorArguments,
			MethodInfo testMethod,
			object?[]? testMethodArguments,
			string? skipReason,
			IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource) :
				base(
					test,
					messageBus,
					testClass,
					constructorArguments,
					testMethod,
					testMethodArguments,
					skipReason,
					aggregator,
					cancellationTokenSource
				)
		{
			BeforeAfterAttributes = Guard.ArgumentNotNull(beforeAfterAttributes);
		}

		/// <summary>
		/// Gets the list of <see cref="BeforeAfterTestAttribute"/>s for this test.
		/// </summary>
		protected IReadOnlyList<BeforeAfterTestAttribute> BeforeAfterAttributes { get; }

		/// <inheritdoc/>
		protected override async ValueTask<Tuple<decimal, string>?> InvokeTestAsync(ExceptionAggregator aggregator)
		{
			var output = string.Empty;
			var testOutputHelper = TestContext.Current?.TestOutputHelper as TestOutputHelper;

			if (testOutputHelper != null)
				testOutputHelper.Initialize(MessageBus, Test);

			var executionTime = await InvokeTestMethodAsync(aggregator, testOutputHelper);

			if (testOutputHelper != null)
			{
				output = testOutputHelper.Output;
				testOutputHelper.Uninitialize();
			}

			return Tuple.Create(executionTime, output);
		}

		/// <summary>
		/// Override this method to invoke the test method.
		/// </summary>
		/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
		/// <param name="testOutputHelper"></param>
		/// <returns>Returns the execution time (in seconds) spent running the test method.</returns>
		protected virtual ValueTask<decimal> InvokeTestMethodAsync(
			ExceptionAggregator aggregator,
			_ITestOutputHelper? testOutputHelper) =>
				XunitTestInvoker.Instance.RunAsync(
					Test,
					TestClass,
					ConstructorArguments,
					TestMethod,
					TestMethodArguments,
					BeforeAfterAttributes,
					MessageBus,
					aggregator,
					CancellationTokenSource
				);

		/// <inheritdoc/>
		protected override void SetTestContext(
			TestEngineStatus testStatus,
			TestState? testState = null) =>
				TestContext.SetForTest(
					Test,
					testStatus,
					CancellationTokenSource.Token,
					testState,
					testStatus == TestEngineStatus.Initializing ? new TestOutputHelper() : TestContext.Current?.TestOutputHelper
				);
	}
}
