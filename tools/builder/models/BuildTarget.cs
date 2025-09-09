using System.Net;

namespace Xunit.BuildTools.Models;

public partial class BuildTarget
{
	public const string CI = nameof(CI);
	public const string DocFX = nameof(DocFX);
	public const string TestConsole = nameof(TestConsole);
	public const string TestMSBuild = nameof(TestMSBuild);
	public const string TestMTP = nameof(TestMTP);
}
