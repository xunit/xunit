using Xunit.v3;

internal class SpyTestProcessBaseWithExitCode : SpyTestProcessBase, ITestProcessWithExitCode
{
	public int? ExitCode { get; set; }
}
