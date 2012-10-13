using System;
using Moq;
using Xunit.Abstractions;

public class MockMethodInfo : Mock<IMethodInfo>
{
    public MockMethodInfo(string methodName = "MockMethod", IAttributeInfo[] attributes = null, IParameterInfo[] parameters = null)
    {
        Setup(t => t.Name).Returns(methodName);
        Setup(t => t.GetCustomAttributes(It.IsAny<Type>())).Returns(attributes ?? new IAttributeInfo[0]);
        Setup(t => t.GetParameters()).Returns(parameters ?? new IParameterInfo[0]);
    }
}
