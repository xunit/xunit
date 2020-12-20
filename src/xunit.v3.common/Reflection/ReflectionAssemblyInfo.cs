using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Internal;

namespace Xunit.Sdk
{
	/// <summary>
	/// Reflection-based implementation of <see cref="IReflectionAssemblyInfo"/>.
	/// </summary>
	public class ReflectionAssemblyInfo : IReflectionAssemblyInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ReflectionAssemblyInfo"/> class.
		/// </summary>
		/// <param name="assembly">The assembly to be wrapped.</param>
		public ReflectionAssemblyInfo(Assembly assembly)
		{
			Assembly = Guard.ArgumentNotNull(nameof(assembly), assembly);
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
				if (Assembly.FullName != null)
					return Assembly.FullName;

				var assemblyPath = AssemblyPath;
				if (assemblyPath != null)
					return Path.GetFileNameWithoutExtension(assemblyPath);

				return "<unknown>";
			}
		}

		/// <inheritdoc/>
		public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
		{
			var attributeType = ReflectionAttributeNameCache.GetType(assemblyQualifiedAttributeTypeName);

			Guard.ArgumentValidNotNull(nameof(assemblyQualifiedAttributeTypeName), $"Could not load type: '{assemblyQualifiedAttributeTypeName}'", attributeType);

			return
				Assembly
					.CustomAttributes
					.Where(attr => attributeType.IsAssignableFrom(attr.AttributeType))
					.OrderBy(attr => attr.AttributeType.Name)
					.Select(a => Reflector.Wrap(a))
					.Cast<IAttributeInfo>()
					.ToList();
		}

		/// <inheritdoc/>
		public ITypeInfo? GetType(string typeName)
		{
			var type = Assembly.GetType(typeName);

			return type == null ? null : Reflector.Wrap(type);
		}

		/// <inheritdoc/>
		public IEnumerable<ITypeInfo> GetTypes(bool includePrivateTypes)
		{
			var selector = includePrivateTypes ? Assembly.DefinedTypes.Select(t => t.AsType()) : Assembly.ExportedTypes;

			try
			{
				return selector.Select(t => Reflector.Wrap(t)).Cast<ITypeInfo>();
			}
			catch (ReflectionTypeLoadException ex)
			{
				return ex.Types.Where(t => t != null).Select(t => Reflector.Wrap(t!)).Cast<ITypeInfo>();
			}
		}

		/// <inheritdoc/>
		public override string? ToString() => Assembly.ToString();
	}
}
