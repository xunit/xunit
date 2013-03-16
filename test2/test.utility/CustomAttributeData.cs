using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

public class CustomAttributeData<TAttribute> : CustomAttributeData, IAttributeInfo
{
    List<CustomAttributeTypedArgument> constructorArgs = new List<CustomAttributeTypedArgument>();
    List<CustomAttributeNamedArgument> namedArgs = new List<CustomAttributeNamedArgument>();

    protected void AddConstructorArgument<TArgumentValue>(TArgumentValue argumentValue)
    {
        constructorArgs.Add(new CustomAttributeTypedArgument(typeof(TArgumentValue), argumentValue));
    }

    protected void AddNamedArgument<TArgumentValue>(string argumentName, TArgumentValue argumentValue)
    {
        var customArgumentValue = new CustomAttributeTypedArgument(typeof(TArgumentValue), argumentValue);
        namedArgs.Add(new CustomAttributeNamedArgument(typeof(TAttribute).GetProperty(argumentName), customArgumentValue));
    }

    public override ConstructorInfo Constructor
    {
        get
        {
            Type[] types = constructorArgs.Select(ca => ca.ArgumentType).ToArray();
            return typeof(TAttribute).GetConstructor(types);
        }
    }

    public override IList<CustomAttributeTypedArgument> ConstructorArguments
    {
        get { return constructorArgs; }
    }

    public override IList<CustomAttributeNamedArgument> NamedArguments
    {
        get { return namedArgs; }
    }

    public IEnumerable<object> GetConstructorArguments()
    {
        return constructorArgs.Select(ca => ca.Value);
    }

    public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
    {
        return Enumerable.Empty<IAttributeInfo>();
    }

    public TValue GetPropertyValue<TValue>(string propertyName)
    {
        return (TValue)namedArgs.Single(na => na.MemberName == propertyName).TypedValue.Value;
    }
}