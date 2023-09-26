using System;
using System.Linq;
using System.Reflection;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// Reflection-based implementation of <see cref="_IReflectionParameterInfo"/>.
/// </summary>
public class ReflectionParameterInfo : _IReflectionParameterInfo
{
	readonly Lazy<_ITypeInfo> parameterType;

	/// <summary>
	/// Initializes a new instance of the <see cref="ReflectionParameterInfo"/> class.
	/// </summary>
	/// <param name="parameterInfo">The parameter to be wrapped.</param>
	public ReflectionParameterInfo(ParameterInfo parameterInfo)
	{
		ParameterInfo = Guard.ArgumentNotNull(parameterInfo);

		parameterType = new(() => Reflector.Wrap(ParameterInfo.ParameterType));
	}

	/// <inheritdoc/>
	public bool IsOptional => ParameterInfo.IsOptional;

	/// <inheritdoc/>
	public string Name => ParameterInfo.Name!;

	/// <inheritdoc/>
	public ParameterInfo ParameterInfo { get; }

	/// <inheritdoc/>
	public _ITypeInfo ParameterType => parameterType.Value;

	/// <inheritdoc/>
	public _IAttributeInfo? GetCustomAttribute(_ITypeInfo attributeType)
	{
		var customAttributeData = ParameterInfo.CustomAttributes.FirstOrDefault(cad => attributeType.Equal(cad.AttributeType));
		if (customAttributeData is null)
			return null;

		return Reflector.Wrap(customAttributeData);
	}
}
