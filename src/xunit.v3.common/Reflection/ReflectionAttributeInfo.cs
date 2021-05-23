using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// Reflection-based implementation of <see cref="_IReflectionAttributeInfo"/>.
	/// </summary>
	public class ReflectionAttributeInfo : _IReflectionAttributeInfo
	{
		static readonly ConcurrentDictionary<Type, AttributeUsageAttribute> attributeUsageCache = new ConcurrentDictionary<Type, AttributeUsageAttribute>();
		static readonly AttributeUsageAttribute defaultAttributeUsageAttribute = new AttributeUsageAttribute(AttributeTargets.All);
		static readonly AttributeUsageAttribute traitAttributeUsageAttribute = new AttributeUsageAttribute(AttributeTargets.All) { AllowMultiple = true };

		/// <summary>
		/// Initializes a new instance of the <see cref="ReflectionAttributeInfo"/> class.
		/// </summary>
		/// <param name="attribute">The attribute to be wrapped.</param>
		public ReflectionAttributeInfo(CustomAttributeData attribute)
		{
			AttributeData = Guard.ArgumentNotNull(nameof(attribute), attribute);
			Attribute = Instantiate(AttributeData);
		}

		/// <inheritdoc/>
		public Attribute Attribute { get; }

		/// <inheritdoc/>
		public CustomAttributeData AttributeData { get; }

		static IEnumerable<object?> Convert(IEnumerable<CustomAttributeTypedArgument> arguments)
		{
			foreach (var argument in arguments)
			{
				var value = argument.Value;

				// Collections are recursively IEnumerable<CustomAttributeTypedArgument> rather than
				// being the exact matching type, so the inner values must be converted.
				if (value is IEnumerable<CustomAttributeTypedArgument> valueAsEnumerable)
					value = Convert(valueAsEnumerable).ToArray();
				else if (value != null && value.GetType() != argument.ArgumentType && argument.ArgumentType.IsEnum)
					value = Enum.Parse(argument.ArgumentType, value.ToString()!);

				if (value != null && value.GetType() != argument.ArgumentType && argument.ArgumentType.IsArray)
					value = Reflector.ConvertArgument(value, argument.ArgumentType);

				yield return value;
			}
		}

		internal static AttributeUsageAttribute GetAttributeUsage(Type attributeType)
		{
			// Can't have a strong type reference because this is part of xunit.v3.core, but this is required for issue #1958.
			if (attributeType.FullName == "Xunit.Sdk.ITraitAttribute")
				return traitAttributeUsageAttribute;

			return attributeUsageCache.GetOrAdd(
				attributeType,
				at => at.GetCustomAttributes(typeof(AttributeUsageAttribute), true).FirstOrDefault() as AttributeUsageAttribute ?? defaultAttributeUsageAttribute
			);
		}

		/// <inheritdoc/>
		public IReadOnlyCollection<object?> GetConstructorArguments() =>
			Convert(AttributeData.ConstructorArguments).CastOrToReadOnlyCollection();

		/// <inheritdoc/>
		public IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
		{
			Guard.ArgumentNotNull(nameof(assemblyQualifiedAttributeTypeName), assemblyQualifiedAttributeTypeName);

			return GetCustomAttributes(Attribute.GetType(), assemblyQualifiedAttributeTypeName);
		}

		internal static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(
			Type type,
			string assemblyQualifiedAttributeTypeName)
		{
			var attributeType = ReflectionAttributeNameCache.GetType(assemblyQualifiedAttributeTypeName);

			Guard.ArgumentValidNotNull(nameof(assemblyQualifiedAttributeTypeName), $"Could not load type: '{assemblyQualifiedAttributeTypeName}'", attributeType);

			return GetCustomAttributes(type, attributeType, GetAttributeUsage(attributeType));
		}

		internal static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(
			Type? type,
			Type attributeType,
			AttributeUsageAttribute attributeUsage)
		{
			var results = Enumerable.Empty<_IAttributeInfo>();

			if (type != null)
			{
				List<ReflectionAttributeInfo>? list = null;
				foreach (var attr in type.CustomAttributes)
				{
					if (attributeType.IsAssignableFrom(attr.AttributeType))
					{
						if (list == null)
							list = new List<ReflectionAttributeInfo>();

						list.Add(new ReflectionAttributeInfo(attr));
					}
				}

				if (list != null)
					list.Sort((left, right) => string.Compare(left.AttributeData.AttributeType.Name, right.AttributeData.AttributeType.Name, StringComparison.Ordinal));

				results = list ?? Enumerable.Empty<_IAttributeInfo>();

				if (attributeUsage.Inherited && (attributeUsage.AllowMultiple || list == null))
					results = results.Concat(GetCustomAttributes(type.BaseType, attributeType, attributeUsage));
			}

			return results.CastOrToReadOnlyCollection();
		}

		/// <inheritdoc/>
		public TValue GetNamedArgument<TValue>(string argumentName)
		{
			foreach (var propInfo in Attribute.GetType().GetRuntimeProperties())
				if (propInfo.Name == argumentName)
					return (TValue)propInfo.GetValue(Attribute)!;

			foreach (var fieldInfo in Attribute.GetType().GetRuntimeFields())
				if (fieldInfo.Name == argumentName)
					return (TValue)fieldInfo.GetValue(Attribute)!;

			throw new ArgumentException($"Could not find property or field named '{argumentName}' on instance of '{Attribute.GetType().FullName}'", nameof(argumentName));
		}

		Attribute Instantiate(CustomAttributeData attributeData)
		{
			var ctorArgs = GetConstructorArguments().ToArray();
			var ctorArgTypes = Reflector.EmptyTypes;
			if (ctorArgs.Length > 0)
			{
				ctorArgTypes = new Type[attributeData.ConstructorArguments.Count];
				for (var i = 0; i < ctorArgTypes.Length; i++)
					ctorArgTypes[i] = attributeData.ConstructorArguments[i].ArgumentType;
			}

			var attribute = (Attribute?)attributeData.Constructor.Invoke(Reflector.ConvertArguments(ctorArgs, ctorArgTypes));
			if (attribute == null)
				throw new ArgumentException($"Unable to create attribute of type '{attributeData.AttributeType.FullName}'", nameof(attributeData));

			var attributeType = attribute.GetType();

			for (var i = 0; i < attributeData.NamedArguments.Count; i++)
			{
				var namedArg = attributeData.NamedArguments[i];
				var typedValue = GetTypedValue(namedArg.TypedValue);
				var memberName = namedArg.MemberName;

				var propInfo = attributeType.GetRuntimeProperty(memberName);
				if (propInfo != null)
					propInfo.SetValue(attribute, typedValue);
				else
				{
					var fieldInfo = attributeType.GetRuntimeField(memberName);
					if (fieldInfo != null)
						fieldInfo.SetValue(attribute, typedValue);
					else
						throw new ArgumentException($"Could not find property or field named '{memberName}' on instance of '{Attribute.GetType().FullName}'", nameof(attributeData));
				}
			}

			return attribute;
		}

		object? GetTypedValue(CustomAttributeTypedArgument arg)
		{
			if (!(arg.Value is IReadOnlyCollection<CustomAttributeTypedArgument> collect))
				return arg.Value;

			var argType = arg.ArgumentType.GetElementType();
			if (argType == null)
				throw new ArgumentException("Could not determine array element type", nameof(arg));

			var destinationArray = Array.CreateInstance(argType, collect.Count);

			if (argType.IsEnum)
				Array.Copy(collect.Select(x => Enum.ToObject(argType, x.Value!)).ToArray(), destinationArray, collect.Count);
			else
				Array.Copy(collect.Select(x => x.Value).ToArray(), destinationArray, collect.Count);

			return destinationArray;
		}

		/// <inheritdoc/>
		public override string? ToString() => Attribute.ToString();
	}
}
