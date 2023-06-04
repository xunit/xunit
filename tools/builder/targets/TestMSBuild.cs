using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestMSBuild,
	BuildTarget.TestFxMSBuild
)]
public static class TestMSBuild { }
