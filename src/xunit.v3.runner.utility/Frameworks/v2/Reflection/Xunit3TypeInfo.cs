using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Provides a class which wraps <see cref="ITypeInfo"/> instances to implement <see cref="_ITypeInfo"/>.
	/// </summary>
	public class Xunit3TypeInfo : _ITypeInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit3TypeInfo"/> class.
		/// </summary>
		/// <param name="v2TypeInfo">The v2 type info to wrap.</param>
		public Xunit3TypeInfo(ITypeInfo v2TypeInfo)
		{
			V2TypeInfo = Guard.ArgumentNotNull(nameof(v2TypeInfo), v2TypeInfo);

			Assembly = new Xunit3AssemblyInfo(V2TypeInfo.Assembly);
			BaseType = V2TypeInfo.BaseType == null ? null : new Xunit3TypeInfo(v2TypeInfo.BaseType);
			Interfaces = V2TypeInfo.Interfaces.Select(i => new Xunit3TypeInfo(i)).CastOrToReadOnlyCollection();

			var typeName = V2TypeInfo.Name;
			var namespaceIdx = typeName.LastIndexOf('.');
			if (namespaceIdx < 0)
				SimpleName = typeName;
			else
			{
				Namespace = typeName.Substring(0, namespaceIdx);
				SimpleName = typeName.Substring(namespaceIdx + 1);

				var innerClassIdx = SimpleName.LastIndexOf('+');
				if (innerClassIdx >= 0)
					SimpleName = SimpleName.Substring(innerClassIdx + 1);
			}
		}

		/// <inheritdoc/>
		public _IAssemblyInfo Assembly { get; }

		/// <inheritdoc/>
		public _ITypeInfo? BaseType { get; }

		/// <inheritdoc/>
		public IReadOnlyCollection<_ITypeInfo> Interfaces { get; }

		/// <inheritdoc/>
		public bool IsAbstract => V2TypeInfo.IsAbstract;

		/// <inheritdoc/>
		public bool IsGenericParameter => V2TypeInfo.IsGenericParameter;

		/// <inheritdoc/>
		public bool IsGenericType => V2TypeInfo.IsGenericType;

		/// <inheritdoc/>
		public bool IsSealed => V2TypeInfo.IsSealed;

		/// <inheritdoc/>
		public bool IsValueType => V2TypeInfo.IsValueType;

		/// <inheritdoc/>
		public string Name => V2TypeInfo.Name;

		/// <inheritdoc/>
		public string? Namespace { get; }

		/// <inheritdoc/>
		public string SimpleName { get; }

		/// <summary>
		/// Gets the underlying xUnit.net v2 <see cref="ITypeInfo"/> that this class is wrapping.
		/// </summary>
		public ITypeInfo V2TypeInfo { get; }

		/// <inheritdoc/>
		public IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName) =>
			V2TypeInfo.GetCustomAttributes(assemblyQualifiedAttributeTypeName).Select(a => new Xunit3AttributeInfo(a)).CastOrToReadOnlyCollection();

		/// <inheritdoc/>
		public IReadOnlyCollection<_ITypeInfo> GetGenericArguments() =>
			V2TypeInfo.GetGenericArguments().Select(t => new Xunit3TypeInfo(t)).CastOrToReadOnlyCollection();

		/// <inheritdoc/>
		public _IMethodInfo? GetMethod(string methodName, bool includePrivateMethod)
		{
			var v2MethodInfo = V2TypeInfo.GetMethod(methodName, includePrivateMethod);
			return v2MethodInfo == null ? null : new Xunit3MethodInfo(v2MethodInfo);
		}

		/// <inheritdoc/>
		public IReadOnlyCollection<_IMethodInfo> GetMethods(bool includePrivateMethods) =>
			V2TypeInfo.GetMethods(includePrivateMethods).Select(m => new Xunit3MethodInfo(m)).CastOrToReadOnlyCollection();
	}
}
