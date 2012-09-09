using System;
using Moq;
using Xunit.Abstractions;

public class MockAssemblyInfo : Mock<IAssemblyInfo>
{
    public MockAssemblyInfo(ITypeInfo[] types = null, IAttributeInfo[] attributes = null)
    {
        Setup(a => a.GetTypes(It.IsAny<bool>())).Returns(types ?? new ITypeInfo[0]);
        Setup(a => a.GetCustomAttributes(It.IsAny<Type>())).Returns(attributes ?? new IAttributeInfo[0]);
    }
}
