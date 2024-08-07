using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestMTP,
	BuildTarget.TestCoreMTP, BuildTarget.TestFxMTP
)]
public static class TestMTP { }
