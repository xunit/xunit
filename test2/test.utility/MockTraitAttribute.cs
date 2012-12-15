using Xunit;

public class MockTraitAttribute : CustomAttributeData<TraitAttribute>
{
    public MockTraitAttribute(string name, string value)
    {
        AddConstructorArgument(name);
        AddConstructorArgument(value);
    }
}