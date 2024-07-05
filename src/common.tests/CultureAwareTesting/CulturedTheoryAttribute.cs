using Xunit.v3;

namespace Xunit;

[XunitTestCaseDiscoverer(typeof(CulturedTheoryAttributeDiscoverer))]
public sealed class CulturedTheoryAttribute : TheoryAttribute
{
	public CulturedTheoryAttribute(params string[] cultures) =>
		Cultures = cultures;

	public string[] Cultures { get; }
}
