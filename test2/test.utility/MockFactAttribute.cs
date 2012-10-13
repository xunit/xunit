using System.Linq;
using Moq;
using Xunit.Abstractions;

public class MockFactAttribute : Mock<IAttributeInfo>
{
    public MockFactAttribute(string displayName = null, string skip = null, int timeout = 0)
    {
        Setup(a => a.GetConstructorArguments()).Returns(Enumerable.Empty<object>());
        Setup(a => a.GetPropertyValue<string>("DisplayName")).Returns(displayName);
        Setup(a => a.GetPropertyValue<string>("Skip")).Returns(skip);
        Setup(a => a.GetPropertyValue<int>("Timeout")).Returns(timeout);
    }
}