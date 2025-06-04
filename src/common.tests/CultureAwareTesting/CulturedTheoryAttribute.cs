using System.Runtime.CompilerServices;
using Xunit.v3;

namespace Xunit;

[XunitTestCaseDiscoverer(typeof(CulturedTheoryAttributeDiscoverer))]
public sealed class CulturedTheoryAttribute(
	string[]? cultures = null,
	[CallerFilePath] string? sourceFilePath = null,
	[CallerLineNumber] int sourceLineNumber = -1) :
		TheoryAttribute(sourceFilePath, sourceLineNumber)
{
	public string[] Cultures { get; } = cultures ?? [];
}
