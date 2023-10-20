using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// Reflection-based implementation of <see cref="_IReflectionAssemblyInfo"/>.
/// </summary>
public class ReflectionAssemblyInfo : _IReflectionAssemblyInfo
{
	_IReflectionAttributeInfo[] additionalAssemblyAttributes;

	/// <summary>
	/// Initializes a new instance of the <see cref="ReflectionAssemblyInfo"/> class.
	/// </summary>
	/// <param name="assembly">The assembly to be wrapped.</param>
	/// <param name="additionalAssemblyAttributes">Additional custom attributes to return for this assembly. These
	/// attributes will be added to the existing assembly-level attributes that already exist. This is typically
	/// only used for unit/acceptance testing purposes.</param>
	public ReflectionAssemblyInfo(
		Assembly assembly,
		params _IReflectionAttributeInfo[] additionalAssemblyAttributes)
	{
		Assembly = Guard.ArgumentNotNull(assembly);
		this.additionalAssemblyAttributes = Guard.ArgumentNotNull(additionalAssemblyAttributes);
	}

	/// <inheritdoc/>
	public Assembly Assembly { get; private set; }

	/// <inheritdoc/>
	public string? AssemblyPath => Assembly.GetLocalCodeBase();

	/// <inheritdoc/>
	public string Name
	{
		get
		{
			if (Assembly.FullName is not null)
				return Assembly.FullName;

			var assemblyPath = AssemblyPath;
			if (assemblyPath is not null)
				return Path.GetFileNameWithoutExtension(assemblyPath);

			return "<unknown>";
		}
	}

	/// <inheritdoc/>
	public IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
	{
		var attributeType = ReflectionAttributeNameCache.GetType(assemblyQualifiedAttributeTypeName);

		Guard.ArgumentNotNull(() => string.Format(CultureInfo.CurrentCulture, "Could not load type: '{0}'", assemblyQualifiedAttributeTypeName), attributeType, nameof(assemblyQualifiedAttributeTypeName));

		return
			additionalAssemblyAttributes
				.Where(customAttribute => attributeType.IsAssignableFrom(customAttribute.AttributeType))
				.Concat(
					Assembly
						.CustomAttributes
						.Where(attr => attributeType.IsAssignableFrom(attr.AttributeType))
						.OrderBy(attr => attr.AttributeType.Name)
						.Select(a => Reflector.Wrap(a))
						.Cast<_IAttributeInfo>()
				)
				.CastOrToReadOnlyCollection();
	}

	/// <inheritdoc/>
	public _ITypeInfo? GetType(string typeName)
	{
		var type = Assembly.GetType(typeName);

		return type is null ? null : Reflector.Wrap(type);
	}

	/// <inheritdoc/>
	public IReadOnlyCollection<_ITypeInfo> GetTypes(bool includePrivateTypes)
	{
		try
		{
			return
				Assembly
					.DefinedTypes
					.Where(t => includePrivateTypes || t.IsPublic || t.IsNestedPublic)
					.Select(t => Reflector.Wrap(t.AsType()))
					.Cast<_ITypeInfo>()
					.CastOrToReadOnlyCollection();
		}
		catch (ReflectionTypeLoadException ex)
		{
			return
				ex
					.Types
					.WhereNotNull()
					.Select(t => Reflector.Wrap(t))
					.Cast<_ITypeInfo>()
					.CastOrToReadOnlyCollection();
		}
	}

	/// <inheritdoc/>
	public override string? ToString() => Assembly.ToString();
}
