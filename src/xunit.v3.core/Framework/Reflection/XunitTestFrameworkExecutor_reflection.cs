using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The implementation of <see cref="ITestFrameworkExecutor"/> that supports execution
/// of unit tests linked against xunit.v3.core.dll.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="XunitTestFrameworkExecutor"/> class.
/// </remarks>
/// <param name="testAssembly">The test assembly.</param>
public class XunitTestFrameworkExecutor(IXunitTestAssembly testAssembly) :
	CoreTestFrameworkExecutor<IXunitTestCase>(testAssembly)
{
	/// <summary>
	/// Gets the test assembly that contains the test.
	/// </summary>
	protected new IXunitTestAssembly TestAssembly { get; } = Guard.ArgumentNotNull(testAssembly);

	/// <inheritdoc/>
	public override async ValueTask RunTestCases(
		IReadOnlyCollection<IXunitTestCase> testCases,
		IMessageSink executionMessageSink,
		ITestFrameworkExecutionOptions executionOptions,
		CancellationToken cancellationToken) =>
			await XunitTestAssemblyRunner.Instance.Run(TestAssembly, testCases, executionMessageSink, executionOptions, cancellationToken);
}
