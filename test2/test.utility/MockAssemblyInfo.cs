using System;
using System.Linq;
using Moq;
using Xunit.Abstractions;

public class MockAssemblyInfo : Mock<IAssemblyInfo>
{
    public MockAssemblyInfo(ITypeInfo[] types = null, IAttributeInfo[] attributes = null)
    {
        Setup(a => a.GetType(It.IsAny<string>())).Returns(types == null ? null : types.FirstOrDefault());
        Setup(a => a.GetTypes(It.IsAny<bool>())).Returns(types ?? new ITypeInfo[0]);
        Setup(a => a.GetCustomAttributes(It.IsAny<Type>())).Returns(attributes ?? new IAttributeInfo[0]);
    }
}
