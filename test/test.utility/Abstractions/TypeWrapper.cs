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

    public bool IsGenericParameter
    {
        get { return inner.IsGenericParameter; }
    }

    public bool IsGenericType
    {
        get { return inner.IsGenericType; }
    }

    public bool IsSealed
    {
        get { return inner.IsSealed; }
    }

    public bool IsValueType
    {
        get { return inner.IsValueType; }
    }

    public string Name
    {
        get { return inner.Name; }
    }

    public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
    {
        return inner.GetCustomAttributes(assemblyQualifiedAttributeTypeName);
    }

    public IEnumerable<ITypeInfo> GetGenericArguments()
    {
        return inner.GetGenericArguments();
    }

    public IMethodInfo GetMethod(string methodName, bool includePrivateMethod)
    {
        return inner.GetMethod(methodName, includePrivateMethod);
    }

    public IEnumerable<IMethodInfo> GetMethods(bool includePrivateMethods)
    {
        return inner.GetMethods(includePrivateMethods);
    }
}
