using System.IO;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.CI,
	BuildTarget.AnalyzeSource, BuildTarget.Test, BuildTarget.DocFX
)]
public static class CI { }
