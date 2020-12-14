using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v1
{
	/// <summary>
	/// Implementation of <see cref="_ITestAssembly"/> and <see cref="IAssemblyInfo"/> for
	/// xUnit.net v1 test assemblies.
	/// </summary>
	public class Xunit1TestAssembly : _ITestAssembly, IAssemblyInfo
	{
		readonly string assemblyFileName;
		readonly string uniqueID;
		readonly Version version = new Version(0, 0, 0, 0);

		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit1TestAssembly"/> class.
		/// </summary>
		/// <param name="assemblyFileName">The assembly under test.</param>
		/// <param name="configFileName">The configuration file name.</param>
		public Xunit1TestAssembly(
			string assemblyFileName,
			string? configFileName = null)
		{
			this.assemblyFileName = Guard.ArgumentNotNullOrEmpty(nameof(assemblyFileName), assemblyFileName);
			ConfigFileName = configFileName;

			uniqueID = UniqueIDGenerator.ForAssembly(Path.GetFileNameWithoutExtension(assemblyFileName), assemblyFileName, configFileName);
		}

		/// <inheritdoc/>
		public IAssemblyInfo Assembly => this;

		/// <inheritdoc/>
		public string? ConfigFileName { get; private set; }

		/// <inheritdoc/>
		public string UniqueID => uniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(UniqueID)} on an uninitialized '{GetType().FullName}' object");

		/// <inheritdoc/>
		public Version Version => version;

		// IAssemblyInfo explicit implementation

		string IAssemblyInfo.AssemblyPath => assemblyFileName ?? throw new InvalidOperationException($"Attempted to get {nameof(IAssemblyInfo)}.{nameof(IAssemblyInfo.AssemblyPath)} on an uninitialized '{GetType().FullName}' object");

		string IAssemblyInfo.Name => Path.GetFileNameWithoutExtension(((IAssemblyInfo)this).AssemblyPath);

		IEnumerable<IAttributeInfo> IAssemblyInfo.GetCustomAttributes(string assemblyQualifiedAttributeTypeName) => Enumerable.Empty<IAttributeInfo>();

		ITypeInfo? IAssemblyInfo.GetType(string typeName) => null;

		IEnumerable<ITypeInfo> IAssemblyInfo.GetTypes(bool includePrivateTypes) => Enumerable.Empty<ITypeInfo>();
	}
}
