using System;
using Moq;
using Xunit.Abstractions;

public class MockTypeInfo : Mock<ITypeInfo>
{
    public MockTypeInfo(string typeName = "MockType", IMethodInfo[] methods = null, IAttributeInfo[] attributes = null)
    {
        Setup(t => t.Name).Returns(typeName);
        Setup(t => t.GetMethods(It.IsAny<bool>())).Returns(methods ?? new IMethodInfo[0]);
        Setup(t => t.GetCustomAttributes(It.IsAny<Type>())).Returns(attributes ?? new IAttributeInfo[0]);
    }
}
