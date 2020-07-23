using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
	/// <summary>
	/// Reflection-based implementation of <see cref="IReflectionTypeInfo"/>.
	/// </summary>
	public class ReflectionTypeInfo : IReflectionTypeInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ReflectionTypeInfo"/> class.
		/// </summary>
		/// <param name="type">The type to wrap.</param>
		public ReflectionTypeInfo(Type type)
		{
			Type = Guard.ArgumentNotNull(nameof(type), type);
		}

		/// <inheritdoc/>
		public IAssemblyInfo Assembly => Reflector.Wrap(Type.Assembly);

		/// <inheritdoc/>
		public ITypeInfo? BaseType => Type.BaseType == null ? null : Reflector.Wrap(Type.BaseType!);

		/// <inheritdoc/>
		public IEnumerable<ITypeInfo> Interfaces => Type.GetInterfaces().Select(i => Reflector.Wrap(i)).ToList();

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
		public Type Type { get; }

		/// <inheritdoc/>
		public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
		{
			Guard.ArgumentNotNull(nameof(assemblyQualifiedAttributeTypeName), assemblyQualifiedAttributeTypeName);

			return ReflectionAttributeInfo.GetCustomAttributes(Type, assemblyQualifiedAttributeTypeName).CastOrToList();
		}

		/// <inheritdoc/>
		public IEnumerable<ITypeInfo> GetGenericArguments() =>
			Type
				.GenericTypeArguments
				.Select(t => Reflector.Wrap(t))
				.ToList();

		/// <inheritdoc/>
		public IMethodInfo? GetMethod(
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
		public IEnumerable<IMethodInfo> GetMethods(bool includePrivateMethods)
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
