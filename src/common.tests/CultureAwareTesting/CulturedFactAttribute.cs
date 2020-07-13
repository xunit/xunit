namespace Xunit.Sdk
{
    [XunitTestCaseDiscoverer(typeof(CulturedFactAttributeDiscoverer))]
    public sealed class CulturedFactAttribute : FactAttribute
    {
        public CulturedFactAttribute(params string[] cultures) { }
    }
}
