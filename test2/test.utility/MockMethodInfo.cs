using System;
using System.Collections.Generic;
using System.Linq;
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

    public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
    {
        var attributeType = GetType(assemblyQualifiedAttributeTypeName);

        foreach (CustomAttributeData attribute in attributes)
            if (attributeType.IsAssignableFrom(attribute.AttributeType))
                yield return Reflector.Wrap(attribute);
    }

    public IEnumerable<IParameterInfo> GetParameters()
    {
        return parameters;
    }

    private static Type GetType(string assemblyQualifiedAttributeTypeName)
    {
        var parts = assemblyQualifiedAttributeTypeName.Split(new[] { ',' }, 2).Select(x => x.Trim()).ToList();
        if (parts.Count == 0)
            return null;

        if (parts.Count == 1)
            return System.Type.GetType(parts[0]);

        var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == parts[1]);
        if (assembly == null)
            return null;

        return assembly.GetType(parts[0]);
    }
}
