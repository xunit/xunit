using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;
[Target(
    BuildTarget.Test,
    BuildTarget.TestCore, BuildTarget.TestFx
)]
public class Test { }
