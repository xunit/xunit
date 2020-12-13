using Xunit.Sdk;

namespace Xunit.v3
{
	[XunitTestCaseDiscoverer(typeof(CulturedFactAttributeDiscoverer))]
	public sealed class CulturedFactAttribute : FactAttribute
	{
		public CulturedFactAttribute(params string[] cultures) { }
	}
}
