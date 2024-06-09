using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestMSBuild,
	BuildTarget.TestCoreMSBuild, BuildTarget.TestFxMSBuild
)]
public static class TestMSBuild { }
