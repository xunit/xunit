using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestCore,
	BuildTarget.TestCoreConsole, BuildTarget.TestCoreMTP, BuildTarget.TestCoreMSBuild
)]
public static class TestCore { }
