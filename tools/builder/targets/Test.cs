using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.Test,
	BuildTarget.TestConsole, BuildTarget.TestMTP, BuildTarget.TestMSBuild, BuildTarget.TestAOT
)]
public class Test { }
