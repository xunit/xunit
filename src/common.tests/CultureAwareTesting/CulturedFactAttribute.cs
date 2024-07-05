using Xunit.v3;

namespace Xunit;

[XunitTestCaseDiscoverer(typeof(CulturedFactAttributeDiscoverer))]
public sealed class CulturedFactAttribute : FactAttribute
{
	public CulturedFactAttribute(params string[] cultures) =>
		Cultures = cultures;

	public string[] Cultures { get; }
}
