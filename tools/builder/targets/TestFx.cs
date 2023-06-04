using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestFx,
	BuildTarget.TestFxConsole, BuildTarget.TestFxMSBuild
)]
public static class TestFx { }
