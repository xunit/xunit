using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// The test method runner for xUnit.net v3 tests.
	/// </summary>
	public class XunitTestMethodRunner : TestMethodRunner<IXunitTestCase>
	{
		readonly object?[] constructorArguments;
		readonly _IMessageSink diagnosticMessageSink;

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTestMethodRunner"/> class.
		/// </summary>
		/// <param name="testMethod">The test method to be run.</param>
		/// <param name="class">The test class that contains the test method.</param>
		/// <param name="method">The test method that contains the tests to be run.</param>
		/// <param name="testCases">The test cases to be run.</param>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="messageBus">The message bus to report run status to.</param>
		/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
		/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
		/// <param name="constructorArguments">The constructor arguments for the test class.</param>
		public XunitTestMethodRunner(
			_ITestMethod testMethod,
			_IReflectionTypeInfo @class,
			_IReflectionMethodInfo method,
			IReadOnlyCollection<IXunitTestCase> testCases,
			_IMessageSink diagnosticMessageSink,
			IMessageBus messageBus,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource,
			object?[] constructorArguments)
				: base(testMethod, @class, method, testCases, messageBus, aggregator, cancellationTokenSource)
		{
			this.constructorArguments = Guard.ArgumentNotNull(nameof(constructorArguments), constructorArguments);
			this.diagnosticMessageSink = Guard.ArgumentNotNull(nameof(diagnosticMessageSink), diagnosticMessageSink);
		}

		/// <inheritdoc/>
		protected override Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase) =>
			testCase.RunAsync(
				diagnosticMessageSink,
				MessageBus,
				constructorArguments,
				new ExceptionAggregator(Aggregator),
				CancellationTokenSource
			);
	}
}
