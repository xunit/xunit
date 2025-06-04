using System.Runtime.CompilerServices;
using Xunit.v3;

namespace Xunit;

[XunitTestCaseDiscoverer(typeof(CulturedFactAttributeDiscoverer))]
public sealed class CulturedFactAttribute(
	string[]? cultures = null,
	[CallerFilePath] string? sourceFilePath = null,
	[CallerLineNumber] int sourceLineNumber = -1) :
		FactAttribute(sourceFilePath, sourceLineNumber)
{
	public string[] Cultures { get; } = cultures ?? [];
}
