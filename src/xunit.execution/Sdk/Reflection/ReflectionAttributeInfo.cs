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

                if (value != null && value.GetType() != argument.ArgumentType && argument.ArgumentType.GetTypeInfo().IsArray)
                    value = Reflector.ConvertArgument(value, argument.ArgumentType);

                yield return value;
            }
        }

        internal static AttributeUsageAttribute GetAttributeUsage(Type attributeType)
        {
            return (AttributeUsageAttribute)attributeType.GetTypeInfo().GetCustomAttributes(typeof(AttributeUsageAttribute), true).FirstOrDefault()
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
                List<ReflectionAttributeInfo> list = null;
                foreach (CustomAttributeData attr in type.GetTypeInfo().CustomAttributes)
                {
                    if (attributeType.GetTypeInfo().IsAssignableFrom(attr.AttributeType.GetTypeInfo()))
                    {
                        if (list == null)
                            list = new List<ReflectionAttributeInfo>();

                        list.Add(new ReflectionAttributeInfo(attr));
                    }
                }

                if (list != null)
                    list.Sort((left, right) => string.Compare(left.AttributeData.AttributeType.Name, right.AttributeData.AttributeType.Name, StringComparison.Ordinal));

                results = list ?? Enumerable.Empty<IAttributeInfo>();

                if (attributeUsage.Inherited && (attributeUsage.AllowMultiple || list == null))
                    results = results.Concat(GetCustomAttributes(type.GetTypeInfo().BaseType, attributeType, attributeUsage));
            }

            return results;
        }

        /// <inheritdoc/>
        public TValue GetNamedArgument<TValue>(string propertyName)
        {
            PropertyInfo propInfo = default(PropertyInfo);
            foreach (var pi in Attribute.GetType().GetRuntimeProperties())
            {
                if (pi.Name == propertyName)
                {
                    propInfo = pi;
                    break;
                }
            }

            Guard.ArgumentValid("propertyName", $"Could not find property {propertyName} on instance of {Attribute.GetType().FullName}", propInfo != null);

            return (TValue)propInfo.GetValue(Attribute, Reflector.EmptyArgs);
        }

        Attribute Instantiate(CustomAttributeData attributeData)
        {
            var ctorArgs = GetConstructorArguments().ToArray();
            Type[] ctorArgTypes = Reflector.EmptyTypes;
            if (ctorArgs.Length > 0)
            {
                ctorArgTypes = new Type[attributeData.ConstructorArguments.Count];
                for (int i = 0; i < ctorArgTypes.Length; i++)
                    ctorArgTypes[i] = attributeData.ConstructorArguments[i].ArgumentType;
            }

            var attribute = (Attribute)Activator.CreateInstance(attributeData.AttributeType, Reflector.ConvertArguments(ctorArgs, ctorArgTypes));

            var ati = attribute.GetType();

            for (int i = 0; i < attributeData.NamedArguments.Count; i++)
            {
                var namedArg = attributeData.NamedArguments[i];
                (ati.GetRuntimeProperty(namedArg.MemberName)).SetValue(attribute, GetTypedValue(namedArg.TypedValue), null);
            }

            return attribute;
        }

        object GetTypedValue(CustomAttributeTypedArgument arg)
        {
            var collect = arg.Value as IReadOnlyCollection<CustomAttributeTypedArgument>;

            if (collect == null)
                return arg.Value;

            var argType = arg.ArgumentType.GetElementType();
            Array destinationArray = Array.CreateInstance(argType, collect.Count);

            if (argType.IsEnum())
                Array.Copy(collect.Select(x => Enum.ToObject(argType, x.Value)).ToArray(), destinationArray, collect.Count);
            else
                Array.Copy(collect.Select(x => x.Value).ToArray(), destinationArray, collect.Count);

            return destinationArray;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Attribute.ToString();
        }
    }
}