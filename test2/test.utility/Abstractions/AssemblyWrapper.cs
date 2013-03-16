using System.Collections.Generic;
using Xunit.Abstractions;

/// <summary>
/// Creates a wrapper around <see cref="IAssemblyInfo"/> to hide reflection implementations.
/// </summary>
public class AssemblyWrapper : IAssemblyInfo
{
    readonly IAssemblyInfo inner;

    public AssemblyWrapper(IAssemblyInfo inner)
    {
        this.inner = inner;
    }

    public string AssemblyPath
    {
        get { return inner.AssemblyPath; }
    }

    public string Name { get { return inner.Name; } }

    public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
    {
        return inner.GetCustomAttributes(assemblyQualifiedAttributeTypeName);
    }

    public ITypeInfo GetType(string typeName)
    {
        return inner.GetType(typeName);
    }

    public IEnumerable<ITypeInfo> GetTypes(bool includePrivateTypes)
    {
        return inner.GetTypes(includePrivateTypes);
    }
}
