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
                else if (value != null && value.GetType() != argument.ArgumentType && argument.ArgumentType.GetTypeInfo().IsEnum)
                    value = Enum.Parse(argument.ArgumentType, value.ToString());

                yield return value;
            }
        }

        internal static AttributeUsageAttribute GetAttributeUsage(Type attributeType)
        {
            return attributeType.GetTypeInfo().GetCustomAttributes(typeof(AttributeUsageAttribute), true)
                                .Cast<AttributeUsageAttribute>()
                                .FirstOrDefault()
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
            return GetCustomAttributes(Attribute.GetType(), assemblyQualifiedAttributeTypeName).ToList();
        }

        internal static IEnumerable<IAttributeInfo> GetCustomAttributes(Type type, string assemblyQualifiedAttributeTypeName)
        {
            Type attributeType = SerializationHelper.GetType(assemblyQualifiedAttributeTypeName);

            return GetCustomAttributes(type, attributeType, GetAttributeUsage(attributeType));
        }

        internal static IEnumerable<IAttributeInfo> GetCustomAttributes(Type type, Type attributeType, AttributeUsageAttribute attributeUsage)
        {
            IEnumerable<IAttributeInfo> results = Enumerable.Empty<IAttributeInfo>();

            if (type != null)
            {
                results = type.GetTypeInfo().CustomAttributes
                                            .Where(attr => attributeType.GetTypeInfo().IsAssignableFrom(attr.AttributeType.GetTypeInfo()))
                                            .OrderBy(attr => attr.AttributeType.Name)
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
            var propInfo = Attribute.GetType().GetRuntimeProperties().FirstOrDefault(pi => pi.Name == propertyName);
            Guard.ArgumentValid("propertyName", String.Format("Could not find property {0} on instance of {1}", propertyName, Attribute.GetType().FullName), propInfo != null);

            return (TValue)propInfo.GetValue(Attribute, new object[0]);
        }

        Attribute Instantiate(CustomAttributeData attributeData)
        {
            var ctorArgs = GetConstructorArguments().ToArray();
            var ctorArgTypes = attributeData.ConstructorArguments.Select(ci => ci.ArgumentType).ToArray();
            var attribute = (Attribute)Activator.CreateInstance(attributeData.AttributeType, Reflector.ConvertArguments(ctorArgs, ctorArgTypes));

            var ati = attribute.GetType();

            foreach (var namedArg in attributeData.NamedArguments)
                (ati.GetRuntimeProperty(namedArg.MemberName)).SetValue(attribute, namedArg.TypedValue.Value, index: null);

            return attribute;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Attribute.ToString();
        }
    }
}