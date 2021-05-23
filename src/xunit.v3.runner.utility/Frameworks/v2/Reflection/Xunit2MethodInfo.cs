using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Provides a class which wraps <see cref="_IMethodInfo"/> instances to implement <see cref="IMethodInfo"/>.
	/// </summary>
	public class Xunit2MethodInfo : LongLivedMarshalByRefObject, IMethodInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit2MethodInfo"/> class.
		/// </summary>
		/// <param name="v3MethodInfo">The v3 method info to wrap.</param>
		public Xunit2MethodInfo(_IMethodInfo v3MethodInfo)
		{
			V3MethodInfo = Guard.ArgumentNotNull(nameof(v3MethodInfo), v3MethodInfo);

			ReturnType = new Xunit2TypeInfo(V3MethodInfo.ReturnType);
			Type = new Xunit2TypeInfo(V3MethodInfo.Type);
		}

		/// <inheritdoc/>
		public bool IsAbstract => V3MethodInfo.IsAbstract;

		/// <inheritdoc/>
		public bool IsGenericMethodDefinition => V3MethodInfo.IsGenericMethodDefinition;

		/// <inheritdoc/>
		public bool IsPublic => V3MethodInfo.IsPublic;

		/// <inheritdoc/>
		public bool IsStatic => V3MethodInfo.IsStatic;

		/// <inheritdoc/>
		public string Name => V3MethodInfo.Name;

		/// <inheritdoc/>
		public ITypeInfo ReturnType { get; }

		/// <inheritdoc/>
		public ITypeInfo Type { get; }

		/// <summary>
		/// Gets the underlying xUnit.net v3 <see cref="_IMethodInfo"/> that this class is wrapping.
		/// </summary>
		public _IMethodInfo V3MethodInfo { get; }

		/// <inheritdoc/>
		public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName) =>
			V3MethodInfo
				.GetCustomAttributes(assemblyQualifiedAttributeTypeName)
				.Select(a => new Xunit2AttributeInfo(a))
				.CastOrToReadOnlyCollection();

		/// <inheritdoc/>
		public IEnumerable<ITypeInfo> GetGenericArguments() =>
			V3MethodInfo
				.GetGenericArguments()
				.Select(t => new Xunit2TypeInfo(t))
				.CastOrToReadOnlyCollection();

		/// <inheritdoc/>
		public IEnumerable<IParameterInfo> GetParameters() =>
			V3MethodInfo
				.GetParameters()
				.Select(p => new Xunit2ParameterInfo(p))
				.CastOrToReadOnlyCollection();

		/// <inheritdoc/>
		public IMethodInfo MakeGenericMethod(params ITypeInfo[] typeArguments) =>
			new Xunit2MethodInfo(V3MethodInfo.MakeGenericMethod(typeArguments.Select(t => new Xunit3TypeInfo(t)).CastOrToArray()));
	}
}
