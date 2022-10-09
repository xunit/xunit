using Xunit.Sdk;

namespace Xunit;

[XunitTestCaseDiscoverer(typeof(CulturedTheoryAttributeDiscoverer))]
public sealed class CulturedTheoryAttribute : TheoryAttribute
{
	public CulturedTheoryAttribute(params string[] cultures) { }
}
