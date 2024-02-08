using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
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
	static readonly ConcurrentDictionary<(string assemblyName, string typeName), AttributeUsageAttribute> attributeUsageCache = new();
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

	/// <inheritdoc/>
	public _ITypeInfo AttributeType => Reflector.Wrap(AttributeData.AttributeType);

	internal static AttributeUsageAttribute GetAttributeUsage(_ITypeInfo attributeType)
	{
		Guard.ArgumentNotNull(attributeType);

		// Can't have a strong type reference because this is part of xunit.v3.core, but this is required for issue #1958.
		if (attributeType.Name == "Xunit.Sdk.ITraitAttribute")
			return traitAttributeUsageAttribute;

		return attributeUsageCache.GetOrAdd(
			(attributeType.Assembly.Name, attributeType.Name),
			_ =>
			{
				if (attributeType is _IReflectionTypeInfo reflectionAttributeType)
					return reflectionAttributeType.Type.GetCustomAttribute<AttributeUsageAttribute>() ?? defaultAttributeUsageAttribute;

				var attributeUsageAttributeInfo = attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute)).SingleOrDefault();
				if (attributeUsageAttributeInfo is null)
					return defaultAttributeUsageAttribute;

				var attributeTargetsObj = attributeUsageAttributeInfo.GetConstructorArguments().FirstOrDefault();
				if (attributeTargetsObj is not AttributeTargets attributeTargets)
					return defaultAttributeUsageAttribute;

				var allowMultiple = attributeUsageAttributeInfo.GetNamedArgument<bool>(nameof(AttributeUsageAttribute.AllowMultiple));
				var inherited = attributeUsageAttributeInfo.GetNamedArgument<bool>(nameof(AttributeUsageAttribute.Inherited));

				return new AttributeUsageAttribute(attributeTargets) { AllowMultiple = allowMultiple, Inherited = inherited };
			}
		);
	}

	/// <inheritdoc/>
	public IReadOnlyCollection<object?> GetConstructorArguments() =>
		(object?[])Reflector.ConvertAttributeArgumentCollection(AttributeData.ConstructorArguments.CastOrToReadOnlyCollection(), typeof(object));

	/// <inheritdoc/>
	public IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(_ITypeInfo attributeType)
	{
		Guard.ArgumentNotNull(attributeType);

		return ReflectionTypeInfo.GetCustomAttributes(AttributeData.AttributeType, attributeType);
	}

	/// <inheritdoc/>
	public TValue? GetNamedArgument<TValue>(string argumentName)
	{
		foreach (var namedArgument in AttributeData.NamedArguments)
			if (namedArgument.MemberName.Equals(argumentName, StringComparison.Ordinal))
			{
				var result = Reflector.ConvertArgument(namedArgument.TypedValue.Value, typeof(TValue));
				return result is null ? default : (TValue)result;
			}

		return default;
	}

	Attribute Instantiate(CustomAttributeData attributeData)
	{
		object?[]? ctorArgs;
		Type? attributeType;

		try
		{
			ctorArgs = GetConstructorArguments().ToArray();
		}
		// Mono throws here when the ctor args can't be matched up, even before we try to invoke the ctor. We don't know exactly
		// why, unfortunately (and the exception does not contain any information), so we have to default to a generic message.
		catch (CustomAttributeFormatException)
		{
			attributeType = attributeData.Constructor.DeclaringType ?? attributeData.Constructor.ReflectedType;
			var fullName = attributeType?.FullName ?? attributeType?.Name ?? "<unknown type>";
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Constructor/initializer arguments for type '{0}' appear to be malformed", fullName), nameof(attributeData));
		}

		var ctorArgTypes = Reflector.EmptyTypes;
		if (ctorArgs.Length > 0)
		{
			ctorArgTypes = new Type[attributeData.ConstructorArguments.Count];
			for (var i = 0; i < ctorArgTypes.Length; i++)
				ctorArgTypes[i] = attributeData.ConstructorArguments[i].ArgumentType;
		}

		var attribute = (Attribute?)attributeData.Constructor.Invoke(Reflector.ConvertArguments(ctorArgs, ctorArgTypes));
		if (attribute is null)
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Unable to create attribute of type '{0}'", attributeData.AttributeType.FullName), nameof(attributeData));

		attributeType = attribute.GetType();

		for (var i = 0; i < attributeData.NamedArguments.Count; i++)
		{
			var namedArg = attributeData.NamedArguments[i];
			var typedValue = Reflector.ConvertArgument(namedArg.TypedValue.Value, namedArg.TypedValue.ArgumentType);
			var memberName = namedArg.MemberName;

			var propInfo = attributeType.GetRuntimeProperty(memberName);
			if (propInfo is not null)
				try
				{
					propInfo.SetValue(attribute, typedValue);
				}
				catch
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Could not set property named '{0}' on instance of '{1}'", memberName, attributeType.FullName), nameof(attributeData));
				}
			else
			{
				var fieldInfo = attributeType.GetRuntimeField(memberName);
				if (fieldInfo is not null)
					try
					{
						fieldInfo.SetValue(attribute, typedValue);
					}
					catch
					{
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Could not set field named '{0}' on instance of '{1}'", memberName, attributeType.FullName), nameof(attributeData));
					}
				else
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Could not find property or field named '{0}' on instance of '{1}'", memberName, attributeType.FullName), nameof(attributeData));
			}
		}

		return attribute;
	}

	/// <inheritdoc/>
	public override string? ToString() => Attribute.ToString();
}
