using System.Collections.Generic;
using Xunit.Abstractions;

/// <summary>
/// Creates a wrapper around <see cref="ITypeInfo"/> to hide reflection implementations.
/// </summary>
public class TypeWrapper : ITypeInfo
{
    readonly ITypeInfo inner;

    public TypeWrapper(ITypeInfo inner)
    {
        this.inner = inner;
    }

    public IAssemblyInfo Assembly
    {
        get { return inner.Assembly; }
    }

    public ITypeInfo BaseType
    {
        get { return inner.BaseType; }
    }

    public IEnumerable<ITypeInfo> Interfaces
    {
        get { return inner.Interfaces; }
    }

    public bool IsAbstract
    {
        get { return inner.IsAbstract; }
    }

    public bool IsSealed
    {
        get { return inner.IsSealed; }
    }

    public string Name
    {
        get { return inner.Name; }
    }

    public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
    {
        return inner.GetCustomAttributes(assemblyQualifiedAttributeTypeName);
    }

    public IEnumerable<IMethodInfo> GetMethods(bool includePrivateMethods)
    {
        return inner.GetMethods(includePrivateMethods);
    }
}