using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

public class MockMethodInfo : IMethodInfo
{
    CustomAttributeData[] attributes;
    IParameterInfo[] parameters;

    public MockMethodInfo(string methodName = "MockMethod", CustomAttributeData[] attributes = null, IParameterInfo[] parameters = null)
    {
        Name = methodName;

        this.attributes = attributes ?? new CustomAttributeData[0];
        this.parameters = parameters ?? new IParameterInfo[0];
    }

    public bool IsAbstract { get; set; }

    public bool IsStatic { get; set; }

    public string Name { get; set; }

    public ITypeInfo ReturnType { get; set; }

    public ITypeInfo Type { get; set; }

    public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
    {
        foreach (CustomAttributeData attribute in attributes)
            if (attributeType.IsAssignableFrom(attribute.AttributeType))
                yield return Reflector.Wrap(attribute);
    }

    public IEnumerable<IParameterInfo> GetParameters()
    {
        return parameters;
    }
}
