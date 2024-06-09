using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestCore,
	BuildTarget.TestCoreConsole, BuildTarget.TestCoreMSBuild
)]
public static class TestCore { }
