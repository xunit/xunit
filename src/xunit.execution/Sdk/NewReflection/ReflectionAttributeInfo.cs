using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

#if NEW_REFLECTION
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
        public ReflectionAttributeInfo(Attribute attribute)
        {
            Attribute = attribute;
        }

        /// <inheritdoc/>
        public Attribute Attribute { get; private set; }



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
                else if (value != null && value.GetType() != argument.ArgumentType && argument.ArgumentType.GetTypeInfo().IsEnum)
                    value = Enum.Parse(argument.ArgumentType, value.ToString());

                yield return value;
            }
        }

        internal static AttributeUsageAttribute GetAttributeUsage(Type attributeType)
        {
            return attributeType.GetTypeInfo().GetCustomAttributes(typeof(AttributeUsageAttribute), true)
                                .Cast<AttributeUsageAttribute>()
                                .SingleOrDefault()
                ?? DefaultAttributeUsageAttribute;
        }

        /// <inheritdoc/>
        public IEnumerable<object> GetConstructorArguments()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            return GetCustomAttributes(Attribute.GetType(), assemblyQualifiedAttributeTypeName).ToList();
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
                results = type.GetTypeInfo()
                              .GetCustomAttributes(attributeType)
                              .Select(Reflector.Wrap)
                              .Cast<IAttributeInfo>();

                if (attributeUsage.Inherited && (attributeUsage.AllowMultiple || !results.Any()))
                    results = results.Concat(GetCustomAttributes(type.GetTypeInfo().BaseType, attributeType, attributeUsage));
            }

            return results;
        }

        /// <inheritdoc/>
        public TValue GetNamedArgument<TValue>(string propertyName)
        {
            PropertyInfo propInfo = Attribute.GetType().GetRuntimeProperty(propertyName);
            Guard.ArgumentValid("propertyName", "Could not find property " + propertyName + " on instance of " + Attribute.GetType().FullName, propInfo != null);

            return (TValue)propInfo.GetValue(Attribute, new object[0]);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Attribute.ToString();
        }
    }
}
#endif