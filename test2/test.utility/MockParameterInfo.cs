using Moq;
using Xunit.Abstractions;

public class MockParameterInfo : Mock<IParameterInfo>
{
    public MockParameterInfo(string name)
    {
        Setup(p => p.Name).Returns(name);
    }
}
