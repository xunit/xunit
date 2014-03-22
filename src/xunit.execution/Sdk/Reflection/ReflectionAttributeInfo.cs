using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Reflection-based implementation of <see cref="IReflectionAttributeInfo"/>.
    /// </summary>
    public class ReflectionAttributeInfo : LongLivedMarshalByRefObject, IReflectionAttributeInfo
    {
        static readonly AttributeUsageAttribute DefaultAttributeUsageAttribute = new AttributeUsageAttribute(AttributeTargets.All);

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionAttributeInfo"/> class.
        /// </summary>
        /// <param name="attribute">The attribute to be wrapped.</param>
        public ReflectionAttributeInfo(CustomAttributeData attribute)
        {
            AttributeData = attribute;
            Attribute = Instantiate(AttributeData);
        }

        /// <inheritdoc/>
        public Attribute Attribute { get; private set; }

        /// <inheritdoc/>
        public CustomAttributeData AttributeData { get; private set; }

        static IEnumerable<object> Convert(IEnumerable<CustomAttributeTypedArgument> arguments)
        {
            foreach (var argument in arguments)
            {
                var value = argument.Value;

                // Collections are recursively IEnumerable<CustomAttributeTypedArgument> rather than
                // being the exact matching type, so the inner values must be converted.
                var valueAsEnumerable = value as IEnumerable<CustomAttributeTypedArgument>;
                if (valueAsEnumerable != null)
                    value = Convert(valueAsEnumerable).ToArray();
                else if (value != null && value.GetType() != argument.ArgumentType && argument.ArgumentType.IsEnum)
                    value = Enum.Parse(argument.ArgumentType, value.ToString());

                yield return value;
            }
        }

        internal static AttributeUsageAttribute GetAttributeUsage(Type attributeType)
        {
            return attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), true)
                                .Cast<AttributeUsageAttribute>()
                                .SingleOrDefault()
                ?? DefaultAttributeUsageAttribute;
        }

        /// <inheritdoc/>
        public IEnumerable<object> GetConstructorArguments()
        {
            return Convert(AttributeData.ConstructorArguments).ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            return GetCustomAttributes(AttributeData.Constructor.ReflectedType, assemblyQualifiedAttributeTypeName).ToList();
        }

        internal static IEnumerable<IAttributeInfo> GetCustomAttributes(Type type, string assemblyQualifiedAttributeTypeName)
        {
            Type attributeType = Reflector.GetType(assemblyQualifiedAttributeTypeName);

            return GetCustomAttributes(type, attributeType, GetAttributeUsage(attributeType));
        }

        internal static IEnumerable<IAttributeInfo> GetCustomAttributes(Type type, Type attributeType, AttributeUsageAttribute attributeUsage)
        {
            IEnumerable<IAttributeInfo> results = Enumerable.Empty<IAttributeInfo>();

            if (type != null)
            {
                results = CustomAttributeData.GetCustomAttributes(type)
                                             .Where(attr => attributeType.IsAssignableFrom(attr.Constructor.ReflectedType))
                                             .OrderBy(attr => attr.Constructor.ReflectedType.Name)
                                             .Select(Reflector.Wrap)
                                             .Cast<IAttributeInfo>();

                if (attributeUsage.Inherited && (attributeUsage.AllowMultiple || !results.Any()))
                    results = results.Concat(GetCustomAttributes(type.BaseType, attributeType, attributeUsage));
            }

            return results;
        }

        /// <inheritdoc/>
        public TValue GetNamedArgument<TValue>(string propertyName)
        {
            PropertyInfo propInfo = Attribute.GetType().GetProperty(propertyName);
            Guard.ArgumentValid("propertyName", "Could not find property " + propertyName + " on instance of " + Attribute.GetType().FullName, propInfo != null);

            return (TValue)propInfo.GetValue(Attribute, new object[0]);
        }

        Attribute Instantiate(CustomAttributeData attributeData)
        {
            Attribute attribute = (Attribute)Activator.CreateInstance(attributeData.Constructor.ReflectedType, GetConstructorArguments().ToArray());

            foreach (CustomAttributeNamedArgument namedArg in attributeData.NamedArguments)
                ((PropertyInfo)namedArg.MemberInfo).SetValue(attribute, namedArg.TypedValue.Value, index: null);

            return attribute;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Attribute.ToString();
        }
    }
}