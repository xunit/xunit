using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Provides a class which wraps <see cref="IAssemblyInfo"/> instances to implement <see cref="_IAssemblyInfo"/>.
	/// </summary>
	public class Xunit3AssemblyInfo : _IAssemblyInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit3AssemblyInfo"/> class.
		/// </summary>
		/// <param name="v2AssemblyInfo">The v2 assembly info to wrap.</param>
		public Xunit3AssemblyInfo(IAssemblyInfo v2AssemblyInfo)
		{
			V2AssemblyInfo = Guard.ArgumentNotNull(nameof(v2AssemblyInfo), v2AssemblyInfo);
		}

		/// <inheritdoc/>
		public string? AssemblyPath => V2AssemblyInfo.AssemblyPath;

		/// <inheritdoc/>
		public string Name => V2AssemblyInfo.Name;

		/// <summary>
		/// Gets the underlying xUnit.net v2 <see cref="IAssemblyInfo"/> that this class is wrapping.
		/// </summary>
		public IAssemblyInfo V2AssemblyInfo { get; }

		/// <inheritdoc/>
		public IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName) =>
			V2AssemblyInfo.GetCustomAttributes(assemblyQualifiedAttributeTypeName).Select(a => new Xunit3AttributeInfo(a)).CastOrToReadOnlyCollection();

		/// <inheritdoc/>
		public _ITypeInfo? GetType(string typeName)
		{
			var v2TypeInfo = V2AssemblyInfo.GetType(typeName);
			return v2TypeInfo == null ? null : new Xunit3TypeInfo(v2TypeInfo);
		}

		/// <inheritdoc/>
		public IReadOnlyCollection<_ITypeInfo> GetTypes(bool includePrivateTypes) =>
			V2AssemblyInfo.GetTypes(includePrivateTypes).Select(t => new Xunit3TypeInfo(t)).CastOrToReadOnlyCollection();
	}
}
