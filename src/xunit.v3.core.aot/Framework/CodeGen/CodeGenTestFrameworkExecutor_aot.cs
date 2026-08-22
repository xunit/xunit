using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The implementation of <see cref="ITestFrameworkExecutor"/> that supports tests registered via
/// code generation.
/// </summary>
/// <param name="testAssembly">The test assembly</param>
public class CodeGenTestFrameworkExecutor(ICodeGenTestAssembly testAssembly) :
	CoreTestFrameworkExecutor<ICodeGenTestCase>(testAssembly)
{
	/// <summary>
	/// Gets the test assembly that contains the test.
	/// </summary>
	protected new ICodeGenTestAssembly TestAssembly { get; } =
		Guard.ArgumentNotNull(testAssembly);

	/// <inheritdoc/>
	public override async ValueTask RunTestCases(
		IReadOnlyCollection<ICodeGenTestCase> testCases,
		IMessageSink executionMessageSink,
		ITestFrameworkExecutionOptions executionOptions,
		CancellationToken cancellationToken) =>
			await CodeGenTestAssemblyRunner.Instance.Run(TestAssembly, testCases, executionMessageSink, executionOptions, cancellationToken);
}
