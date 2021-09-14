using System.Collections.Generic;
using Xunit.Abstractions;

/// <summary>
/// Creates a wrapper around <see cref="IAttributeInfo"/> to hide reflection implementations.
/// </summary>
public class AttributeWrapper : IAttributeInfo
{
    readonly IAttributeInfo inner;

    public AttributeWrapper(IAttributeInfo inner)
    {
        this.inner = inner;
    }

    public IEnumerable<object> GetConstructorArguments()
    {
        return inner.GetConstructorArguments();
    }

    public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
    {
        return inner.GetCustomAttributes(assemblyQualifiedAttributeTypeName);
    }

    public TValue GetNamedArgument<TValue>(string propertyName)
    {
        return inner.GetNamedArgument<TValue>(propertyName);
    }
}
