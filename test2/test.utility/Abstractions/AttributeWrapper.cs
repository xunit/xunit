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

    public TValue GetPropertyValue<TValue>(string propertyName)
    {
        return inner.GetPropertyValue<TValue>(propertyName);
    }
}