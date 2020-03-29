using Xunit.Sdk;

namespace Xunit
{
    [XunitTestCaseDiscoverer("TestUtility.CulturedFactAttributeDiscoverer", "test.utility")]
    public sealed class CulturedFactAttribute : FactAttribute
    {
        public CulturedFactAttribute(params string[] cultures) { }
    }
}
