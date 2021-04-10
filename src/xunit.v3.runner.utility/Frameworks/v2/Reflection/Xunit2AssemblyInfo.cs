using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Provides a class which wraps <see cref="_IAssemblyInfo"/> instances to implement <see cref="IAssemblyInfo"/>.
	/// </summary>
	public class Xunit2AssemblyInfo : LongLivedMarshalByRefObject, IAssemblyInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit2AssemblyInfo"/> class.
		/// </summary>
		/// <param name="v3AssemblyInfo">The v3 assembly info to wrap.</param>
		public Xunit2AssemblyInfo(_IAssemblyInfo v3AssemblyInfo)
		{
			V3AssemblyInfo = Guard.ArgumentNotNull(nameof(v3AssemblyInfo), v3AssemblyInfo);
		}

		/// <inheritdoc/>
		public string? AssemblyPath => V3AssemblyInfo.AssemblyPath;

		/// <inheritdoc/>
		public string Name => V3AssemblyInfo.Name;

		/// <summary>
		/// Gets the underlying xUnit.net v3 <see cref="_IAssemblyInfo"/> that this class is wrapping.
		/// </summary>
		public _IAssemblyInfo V3AssemblyInfo { get; }

		/// <inheritdoc/>
		public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName) =>
			V3AssemblyInfo
				.GetCustomAttributes(assemblyQualifiedAttributeTypeName)
				.Select(a => new Xunit2AttributeInfo(a))
				.CastOrToArray();

		/// <inheritdoc/>
		public ITypeInfo? GetType(string typeName)
		{
			var v3TypeInfo = V3AssemblyInfo.GetType(typeName);
			return v3TypeInfo == null ? null : new Xunit2TypeInfo(v3TypeInfo);
		}

		/// <inheritdoc/>
		public IEnumerable<ITypeInfo> GetTypes(bool includePrivateTypes) =>
			V3AssemblyInfo
				.GetTypes(includePrivateTypes)
				.Select(t => new Xunit2TypeInfo(t))
				.CastOrToArray();
	}
}
