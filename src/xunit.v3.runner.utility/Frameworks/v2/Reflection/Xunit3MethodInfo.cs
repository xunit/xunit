using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Provides a class which wraps <see cref="IMethodInfo"/> instances to implement <see cref="_IMethodInfo"/>.
	/// </summary>
	public class Xunit3MethodInfo : _IMethodInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit3MethodInfo"/> class.
		/// </summary>
		/// <param name="v2MethodInfo">The v2 method info to wrap.</param>
		public Xunit3MethodInfo(IMethodInfo v2MethodInfo)
		{
			V2MethodInfo = Guard.ArgumentNotNull(nameof(v2MethodInfo), v2MethodInfo);

			ReturnType = new Xunit3TypeInfo(V2MethodInfo.ReturnType);
			Type = new Xunit3TypeInfo(V2MethodInfo.Type);
		}

		/// <inheritdoc/>
		public bool IsAbstract => V2MethodInfo.IsAbstract;

		/// <inheritdoc/>
		public bool IsGenericMethodDefinition => V2MethodInfo.IsGenericMethodDefinition;

		/// <inheritdoc/>
		public bool IsPublic => V2MethodInfo.IsPublic;

		/// <inheritdoc/>
		public bool IsStatic => V2MethodInfo.IsStatic;

		/// <inheritdoc/>
		public string Name => V2MethodInfo.Name;

		/// <inheritdoc/>
		public _ITypeInfo ReturnType { get; }

		/// <inheritdoc/>
		public _ITypeInfo Type { get; }

		/// <summary>
		/// Gets the underlying xUnit.net v2 <see cref="IMethodInfo"/> that this class is wrapping.
		/// </summary>
		public IMethodInfo V2MethodInfo { get; }

		/// <inheritdoc/>
		public IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName) =>
			V2MethodInfo.GetCustomAttributes(assemblyQualifiedAttributeTypeName).Select(a => new Xunit3AttributeInfo(a)).CastOrToReadOnlyCollection();

		/// <inheritdoc/>
		public IReadOnlyCollection<_ITypeInfo> GetGenericArguments() =>
			V2MethodInfo.GetGenericArguments().Select(t => new Xunit3TypeInfo(t)).CastOrToReadOnlyCollection();

		/// <inheritdoc/>
		public IReadOnlyCollection<_IParameterInfo> GetParameters() =>
			V2MethodInfo.GetParameters().Select(p => new Xunit3ParameterInfo(p)).CastOrToReadOnlyCollection();

		/// <inheritdoc/>
		public _IMethodInfo MakeGenericMethod(params _ITypeInfo[] typeArguments) =>
			new Xunit3MethodInfo(V2MethodInfo.MakeGenericMethod(typeArguments.Select(t => new Xunit2TypeInfo(t)).ToArray()));
	}
}
