namespace Xunit.Sdk
{
	[XunitTestCaseDiscoverer(typeof(CulturedTheoryAttributeDiscoverer))]
	public sealed class CulturedTheoryAttribute : TheoryAttribute
	{
		public CulturedTheoryAttribute(params string[] cultures) { }
	}
}
