using Xunit.Sdk;

namespace Xunit;

[XunitTestCaseDiscoverer(typeof(CulturedFactAttributeDiscoverer))]
public sealed class CulturedFactAttribute : FactAttribute
{
	public CulturedFactAttribute(params string[] cultures) { }
}
