using Xunit.Sdk;

namespace Xunit
{
    [XunitTestCaseDiscoverer("TestUtility.CulturedTheoryAttributeDiscoverer", "test.utility")]
    public sealed class CulturedTheoryAttribute : TheoryAttribute
    {
        public CulturedTheoryAttribute(params string[] cultures) { }
    }
}
