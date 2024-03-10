using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestCore,
	BuildTarget.TestCoreConsole, BuildTarget.TestCoreDotNetTest
)]
public static class TestCore { }
