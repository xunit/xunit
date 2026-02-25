using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

partial class XunitRunnerHelper
{
	/// <summary>
	/// Runs a single test case using the <see cref="CodeGenTestCaseRunner"/> after enumerating
	/// all tests.
	/// </summary>
	/// <param name="testCase">The test case to run</param>
	/// <param name="messageBus">The message bus to send the messages to</param>
	/// <param name="cancellationTokenSource">The cancellation token source to cancel if requested</param>
	/// <param name="aggregator">The exception aggregator to record exceptions to</param>
	/// <param name="explicitOption">A flag to indicate which types of tests to run (non-explicit, explicit, or both)</param>
	/// <param name="classFixtureMappings">The mapping of class fixture types to fixtures.</param>
	public static ValueTask<RunSummary> RunCodeGenTestCase(
		ICodeGenTestCase testCase,
		IMessageBus messageBus,
		CancellationTokenSource cancellationTokenSource,
		ExceptionAggregator aggregator,
		ExplicitOption explicitOption,
		FixtureMappingManager classFixtureMappings) =>
			RunCoreTestCase(
				Guard.ArgumentNotNull(testCase),
				messageBus,
				aggregator,
				testCase.CreateTests,
				tests => CodeGenTestCaseRunner.Instance.Run(
					testCase,
					tests,
					explicitOption,
					messageBus,
					aggregator,
					testCase.TestCaseDisplayName,
					testCase.SkipReason,
					cancellationTokenSource,
					classFixtureMappings
				),
				cancellationTokenSource
			);

	/// <summary>
	/// Please use <see cref="RunCodeGenTestCase"/> in Native AOT
	/// </summary>
	[Obsolete("Please use RunCodeGenTestCase in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static ValueTask<RunSummary> RunXunitTestCase(
		IXunitTestCase testCase,
		IMessageBus messageBus,
		CancellationTokenSource cancellationTokenSource,
		ExceptionAggregator aggregator,
		ExplicitOption explicitOption,
		object?[] constructorArguments) =>
			throw new PlatformNotSupportedException("Please use RunCodeGenTestCase in Native AOT");
}
