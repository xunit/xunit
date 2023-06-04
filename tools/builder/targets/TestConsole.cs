using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestConsole,
	BuildTarget.TestCoreConsole, BuildTarget.TestFxConsole
)]
public static class TestConsole { }
