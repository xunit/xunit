using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The implementation of <see cref="ITestFrameworkExecutor"/> that supports tests registered via
/// code generation.
/// </summary>
/// <param name="testAssembly">The test assembly</param>
public class CodeGenTestFrameworkExecutor(ICodeGenTestAssembly testAssembly) :
	TestFrameworkExecutor<ICodeGenTestCase>(testAssembly)
{
	/// <summary>
	/// Gets the test assembly that contains the test.
	/// </summary>
	protected new ICodeGenTestAssembly TestAssembly { get; } =
		Guard.ArgumentNotNull(testAssembly);

	/// <inheritdoc/>
	protected override ITestFrameworkDiscoverer CreateDiscoverer() =>
		new CodeGenTestFrameworkDiscoverer(testAssembly);

	/// <inheritdoc/>
	public override async ValueTask RunTestCases(
		IReadOnlyCollection<ICodeGenTestCase> testCases,
		IMessageSink executionMessageSink,
		ITestFrameworkExecutionOptions executionOptions,
		CancellationToken cancellationToken)
	{
		SetEnvironment(EnvironmentVariables.AssertEquivalentMaxDepth, executionOptions.AssertEquivalentMaxDepth());
		SetEnvironment(EnvironmentVariables.PrintMaxEnumerableLength, executionOptions.PrintMaxEnumerableLength());
		SetEnvironment(EnvironmentVariables.PrintMaxObjectDepth, executionOptions.PrintMaxObjectDepth());
		SetEnvironment(EnvironmentVariables.PrintMaxObjectMemberCount, executionOptions.PrintMaxObjectMemberCount());
		SetEnvironment(EnvironmentVariables.PrintMaxStringLength, executionOptions.PrintMaxStringLength());

		await CodeGenTestAssemblyRunner.Instance.Run(
			TestAssembly,
			testCases,
			executionMessageSink,
			executionOptions,
			cancellationToken
		);
	}

	static void SetEnvironment(
		string environmentVariableName,
		int? value)
	{
		if (value.HasValue)
			Environment.SetEnvironmentVariable(environmentVariableName, value.Value.ToString(CultureInfo.InvariantCulture));
	}
}
