using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base implementation derived from <see cref="TestFrameworkExecutor{TTestCase}"/> which contains common
/// code used for both reflection and native AOT test execution.
/// </summary>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ICoreTestCase"/>.</typeparam>
/// <param name="testAssembly">The test assembly.</param>
public abstract class CoreTestFrameworkExecutor<TTestCase>(ICoreTestAssembly testAssembly) :
	TestFrameworkExecutor<TTestCase>(testAssembly)
		where TTestCase : ICoreTestCase
{
	internal override ValueTask RunTestCasesInternal(
		IReadOnlyCollection<ITestCase> testCases,
		IMessageSink executionMessageSink,
		ITestFrameworkExecutionOptions executionOptions,
		CancellationToken? cancellationToken)
	{
		SetEnvironment(EnvironmentVariables.AssertEquivalentMaxDepth, executionOptions.AssertEquivalentMaxDepth());
		SetEnvironment(EnvironmentVariables.PrintMaxEnumerableLength, executionOptions.PrintMaxEnumerableLength());
		SetEnvironment(EnvironmentVariables.PrintMaxObjectDepth, executionOptions.PrintMaxObjectDepth());
		SetEnvironment(EnvironmentVariables.PrintMaxObjectMemberCount, executionOptions.PrintMaxObjectMemberCount());
		SetEnvironment(EnvironmentVariables.PrintMaxStringLength, executionOptions.PrintMaxStringLength());

		return base.RunTestCasesInternal(testCases, executionMessageSink, executionOptions, cancellationToken);
	}

	static void SetEnvironment(
		string environmentVariableName,
		int? value)
	{
		if (value.HasValue)
			Environment.SetEnvironmentVariable(environmentVariableName, value.Value.ToString(CultureInfo.InvariantCulture));
	}
}
