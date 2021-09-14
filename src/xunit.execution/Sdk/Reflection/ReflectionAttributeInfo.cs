using System;
using System.Collections.Concurrent;
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
        static readonly ConcurrentDictionary<Type, AttributeUsageAttribute> attributeUsageCache = new ConcurrentDictionary<Type, AttributeUsageAttribute>();

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
            return attributeUsageCache.GetOrAdd(attributeType,
                at => (AttributeUsageAttribute)at.GetTypeInfo().GetCustomAttributes(typeof(AttributeUsageAttribute), true).FirstOrDefault()
                      ?? DefaultAttributeUsageAttribute);
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
            Type attributeType = ReflectionAttributeNameCache.GetType(assemblyQualifiedAttributeTypeName);

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
        public TValue GetNamedArgument<TValue>(string argumentName)
        {
            foreach (var propInfo in Attribute.GetType().GetRuntimeProperties())
                if (propInfo.Name == argumentName)
                    return (TValue)propInfo.GetValue(Attribute);

            foreach (var fieldInfo in Attribute.GetType().GetRuntimeFields())
                if (fieldInfo.Name == argumentName)
                    return (TValue)fieldInfo.GetValue(Attribute);

            throw new ArgumentException($"Could not find property or field named '{argumentName}' on instance of '{Attribute.GetType().FullName}'", nameof(argumentName));
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
                var typedValue = GetTypedValue(namedArg.TypedValue);
                var memberName = namedArg.MemberName;

                var propInfo = ati.GetRuntimeProperty(memberName);
                if (propInfo != null)
                    propInfo.SetValue(attribute, typedValue);
                else
                {
                    var fieldInfo = ati.GetRuntimeField(memberName);
                    if (fieldInfo != null)
                        fieldInfo.SetValue(attribute, typedValue);
                    else
                        throw new ArgumentException($"Could not find property or field named '{memberName}' on instance of '{Attribute.GetType().FullName}'", nameof(attributeData));
                }
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
