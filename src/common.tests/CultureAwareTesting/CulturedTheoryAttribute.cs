using Xunit.Sdk;

namespace Xunit.v3
{
	[XunitTestCaseDiscoverer(typeof(CulturedTheoryAttributeDiscoverer))]
	public sealed class CulturedTheoryAttribute : TheoryAttribute
	{
		public CulturedTheoryAttribute(params string[] cultures) { }
	}
}
