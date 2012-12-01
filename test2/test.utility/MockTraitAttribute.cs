using Xunit;

public class MockTraitAttribute : CustomAttributeData<Trait2Attribute>
{
    public MockTraitAttribute(string name, string value)
    {
        AddConstructorArgument(name);
        AddConstructorArgument(value);
    }
}