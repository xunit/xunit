using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// Reflection-based implementation of <see cref="_IReflectionAttributeInfo"/>.
/// </summary>
public class ReflectionAttributeInfo : _IReflectionAttributeInfo
{
	readonly Lazy<Attribute> attribute;
	static readonly ConcurrentDictionary<Type, AttributeUsageAttribute> attributeUsageCache = new();
	static readonly AttributeUsageAttribute defaultAttributeUsageAttribute = new(AttributeTargets.All);
	static readonly AttributeUsageAttribute traitAttributeUsageAttribute = new(AttributeTargets.All) { AllowMultiple = true };

	/// <summary>
	/// Initializes a new instance of the <see cref="ReflectionAttributeInfo"/> class.
	/// </summary>
	/// <param name="attributeData">The attribute to be wrapped.</param>
	public ReflectionAttributeInfo(CustomAttributeData attributeData)
	{
		AttributeData = Guard.ArgumentNotNull(attributeData);
		attribute = new(() => Instantiate(AttributeData));
	}

	/// <inheritdoc/>
	public Attribute Attribute => attribute.Value;

	/// <inheritdoc/>
	public CustomAttributeData AttributeData { get; }

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
		(object?[])Reflector.ConvertAttributeArgumentCollection(AttributeData.ConstructorArguments.CastOrToReadOnlyCollection(), typeof(object));

	/// <inheritdoc/>
	public IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
	{
		Guard.ArgumentNotNull(assemblyQualifiedAttributeTypeName);

		return GetCustomAttributes(Attribute.GetType(), assemblyQualifiedAttributeTypeName);
	}

	internal static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(
		Type type,
		string assemblyQualifiedAttributeTypeName)
	{
		var attributeType = ReflectionAttributeNameCache.GetType(assemblyQualifiedAttributeTypeName);

		Guard.ArgumentNotNull($"Could not load type: '{assemblyQualifiedAttributeTypeName}'", attributeType, nameof(assemblyQualifiedAttributeTypeName));

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
					list ??= new List<ReflectionAttributeInfo>();
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
	public TValue? GetNamedArgument<TValue>(string argumentName)
	{
		foreach (var namedArgument in AttributeData.NamedArguments)
			if (namedArgument.MemberName.Equals(argumentName, StringComparison.Ordinal))
			{
				var result = Reflector.ConvertArgument(namedArgument.TypedValue.Value, typeof(TValue));
				return result == null ? default : (TValue)result;
			}

		return default;
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
			var typedValue = Reflector.ConvertArgument(namedArg.TypedValue.Value, namedArg.TypedValue.ArgumentType);
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

	/// <inheritdoc/>
	public override string? ToString() => Attribute.ToString();
}
