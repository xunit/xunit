using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// Reflection-based implementation of <see cref="_IReflectionTypeInfo"/>.
	/// </summary>
	public class ReflectionTypeInfo : _IReflectionTypeInfo
	{
		readonly Lazy<_IAssemblyInfo> assembly;
		readonly Lazy<_ITypeInfo?> baseType;
		readonly Lazy<IEnumerable<_ITypeInfo>> interfaces;

		/// <summary>
		/// Initializes a new instance of the <see cref="ReflectionTypeInfo"/> class.
		/// </summary>
		/// <param name="type">The type to wrap.</param>
		public ReflectionTypeInfo(Type type)
		{
			Type = Guard.ArgumentNotNull(nameof(type), type);

			assembly = new(() => Reflector.Wrap(Type.Assembly));
			baseType = new(() => Type.BaseType == null ? null : Reflector.Wrap(Type.BaseType!));
			interfaces = new(() => Type.GetInterfaces().Select(i => Reflector.Wrap(i)).ToList());
		}

		/// <inheritdoc/>
		public _IAssemblyInfo Assembly => assembly.Value;

		/// <inheritdoc/>
		public _ITypeInfo? BaseType => baseType.Value;

		/// <inheritdoc/>
		public IEnumerable<_ITypeInfo> Interfaces => interfaces.Value;

		/// <inheritdoc/>
		public bool IsAbstract => Type.IsAbstract;

		/// <inheritdoc/>
		public bool IsGenericParameter => Type.IsGenericParameter;

		/// <inheritdoc/>
		public bool IsGenericType => Type.IsGenericType;

		/// <inheritdoc/>
		public bool IsSealed => Type.IsSealed;

		/// <inheritdoc/>
		public bool IsValueType => Type.IsValueType;

		/// <inheritdoc/>
		public string Name => Type.FullName ?? Type.Name;

		/// <inheritdoc/>
		public string? Namespace => Type.Namespace;

		/// <inheritdoc/>
		public string SimpleName => Type.Name;

		/// <inheritdoc/>
		public Type Type { get; }

		/// <inheritdoc/>
		public IEnumerable<_IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
		{
			Guard.ArgumentNotNull(nameof(assemblyQualifiedAttributeTypeName), assemblyQualifiedAttributeTypeName);

			return ReflectionAttributeInfo.GetCustomAttributes(Type, assemblyQualifiedAttributeTypeName).CastOrToList();
		}

		/// <inheritdoc/>
		public IEnumerable<_ITypeInfo> GetGenericArguments() =>
			Type
				.GenericTypeArguments
				.Select(t => Reflector.Wrap(t))
				.ToList();

		/// <inheritdoc/>
		public _IMethodInfo? GetMethod(
			string methodName,
			bool includePrivateMethod)
		{
			Guard.ArgumentNotNull(nameof(methodName), methodName);

			var method =
				Type
					.GetRuntimeMethods()
					.FirstOrDefault(m => (includePrivateMethod || m.IsPublic && m.DeclaringType != typeof(object)) && m.Name == methodName);

			if (method == null)
				return null;

			return Reflector.Wrap(method);
		}

		/// <inheritdoc/>
		public IEnumerable<_IMethodInfo> GetMethods(bool includePrivateMethods)
		{
			var methodInfos = Type.GetRuntimeMethods();

			if (!includePrivateMethods)
				methodInfos = methodInfos.Where(m => m.IsPublic);

			return methodInfos.Select(m => Reflector.Wrap(m)).ToList();
		}

		/// <inheritdoc/>
		public override string? ToString() => Type.ToString();
	}
}
