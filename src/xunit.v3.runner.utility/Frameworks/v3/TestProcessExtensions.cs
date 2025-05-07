namespace Xunit.v3;

/// <summary>
/// Extension methods for <see cref="ITestProcessBase"/> and friends.
/// </summary>
public static class TestProcessExtensions
{
	/// <summary>
	/// Determines if the test process implements <see cref="ITestProcessWithExitCode"/>, and if it does,
	/// returns <see cref="ITestProcessWithExitCode.ExitCode"/>; if not, returns <c>null</c>.
	/// </summary>
	public static int? TryGetExitCode(this ITestProcessBase testProcess) =>
		(testProcess as ITestProcessWithExitCode)?.ExitCode;
}
