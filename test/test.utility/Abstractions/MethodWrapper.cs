using System.Collections.Generic;
using Xunit.Abstractions;

/// <summary>
/// Creates a wrapper around <see cref="IMethodInfo"/> to hide reflection implementations.
/// </summary>
public class MethodWrapper : IMethodInfo
{
    readonly IMethodInfo inner;

    public MethodWrapper(IMethodInfo inner)
    {
        this.inner = inner;
    }

    public bool IsAbstract
    {
        get { return inner.IsAbstract; }
    }

    public bool IsGenericMethodDefinition
    {
        get { return inner.IsGenericMethodDefinition; }
    }

    public bool IsPublic
    {
        get { return inner.IsPublic; }
    }

    public bool IsStatic
    {
        get { return inner.IsStatic; }
    }

    public string Name
    {
        get { return inner.Name; }
    }

    public ITypeInfo ReturnType
    {
        get { return inner.ReturnType; }
    }

    public ITypeInfo Type
    {
        get { return inner.Type; }
    }

    public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
    {
        return inner.GetCustomAttributes(assemblyQualifiedAttributeTypeName);
    }

    public IEnumerable<ITypeInfo> GetGenericArguments()
    {
        return inner.GetGenericArguments();
    }

    public IEnumerable<IParameterInfo> GetParameters()
    {
        return inner.GetParameters();
    }

    public IMethodInfo MakeGenericMethod(params ITypeInfo[] typeArguments)
    {
        return inner.MakeGenericMethod(typeArguments);
    }
}
