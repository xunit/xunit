using Moq;
using Xunit.Abstractions;

public class MockTraitAttribute : Mock<IAttributeInfo>
{
    public MockTraitAttribute(string name, string value)
    {
        Setup(a => a.GetPropertyValue<string>("Name")).Returns(name);
        Setup(a => a.GetPropertyValue<string>("Value")).Returns(value);
    }
}
