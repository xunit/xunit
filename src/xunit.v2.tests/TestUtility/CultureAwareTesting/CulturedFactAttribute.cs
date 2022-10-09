using Xunit.Sdk;

namespace Xunit
{
	[XunitTestCaseDiscoverer("TestUtility.CulturedFactAttributeDiscoverer", "xunit.v2.tests")]
	public sealed class CulturedFactAttribute : FactAttribute
	{
		public CulturedFactAttribute(params string[] cultures) { }
	}
}
