using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Provides a class which wraps <see cref="_ITypeInfo"/> instances to implement <see cref="ITypeInfo"/>.
	/// </summary>
	public class Xunit2TypeInfo : LongLivedMarshalByRefObject, ITypeInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit2TypeInfo"/> class.
		/// </summary>
		/// <param name="v3TypeInfo">The v3 type info to wrap.</param>
		public Xunit2TypeInfo(_ITypeInfo v3TypeInfo)
		{
			V3TypeInfo = Guard.ArgumentNotNull(nameof(v3TypeInfo), v3TypeInfo);

			Assembly = new Xunit2AssemblyInfo(V3TypeInfo.Assembly);
			BaseType = V3TypeInfo.BaseType == null ? null : new Xunit2TypeInfo(V3TypeInfo.BaseType);
			Interfaces = V3TypeInfo.Interfaces.Select(i => new Xunit2TypeInfo(i)).ToList();
		}

		/// <inheritdoc/>
		public IAssemblyInfo Assembly { get; }

		/// <inheritdoc/>
		public ITypeInfo? BaseType { get; }

		/// <inheritdoc/>
		public IEnumerable<ITypeInfo> Interfaces { get; }

		/// <inheritdoc/>
		public bool IsAbstract => V3TypeInfo.IsAbstract;

		/// <inheritdoc/>
		public bool IsGenericParameter => V3TypeInfo.IsGenericParameter;

		/// <inheritdoc/>
		public bool IsGenericType => V3TypeInfo.IsGenericType;

		/// <inheritdoc/>
		public bool IsSealed => V3TypeInfo.IsSealed;

		/// <inheritdoc/>
		public bool IsValueType => V3TypeInfo.IsValueType;

		/// <inheritdoc/>
		public string Name => V3TypeInfo.Name;

		/// <summary>
		/// Gets the underlying xUnit.net v3 <see cref="_ITypeInfo"/> that this class is wrapping.
		/// </summary>
		public _ITypeInfo V3TypeInfo { get; }

		/// <inheritdoc/>
		public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName) =>
			V3TypeInfo
				.GetCustomAttributes(assemblyQualifiedAttributeTypeName)
				.Select(a => new Xunit2AttributeInfo(a))
				.CastOrToReadOnlyCollection();

		/// <inheritdoc/>
		public IEnumerable<ITypeInfo> GetGenericArguments() =>
			V3TypeInfo
				.GetGenericArguments()
				.Select(t => new Xunit2TypeInfo(t))
				.CastOrToReadOnlyCollection();

		/// <inheritdoc/>
		public IMethodInfo? GetMethod(string methodName, bool includePrivateMethod)
		{
			var v3MethodInfo = V3TypeInfo.GetMethod(methodName, includePrivateMethod);
			return v3MethodInfo == null ? null : new Xunit2MethodInfo(v3MethodInfo);
		}

		/// <inheritdoc/>
		public IEnumerable<IMethodInfo> GetMethods(bool includePrivateMethods) =>
			V3TypeInfo
				.GetMethods(includePrivateMethods)
				.Select(m => new Xunit2MethodInfo(m))
				.CastOrToReadOnlyCollection();
	}
}
