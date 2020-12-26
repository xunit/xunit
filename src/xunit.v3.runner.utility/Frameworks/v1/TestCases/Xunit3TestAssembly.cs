using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v1
{
	/// <summary>
	/// Implementation of <see cref="_ITestAssembly"/> and <see cref="_IAssemblyInfo"/> for
	/// xUnit.net v1 test assemblies.
	/// </summary>
	public class Xunit3TestAssembly : _ITestAssembly, _IAssemblyInfo
	{
		readonly string assemblyFileName;
		readonly string uniqueID;
		readonly Version version = new Version(0, 0, 0, 0);

		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit3TestAssembly"/> class.
		/// </summary>
		/// <param name="assemblyFileName">The assembly under test.</param>
		/// <param name="configFileName">The configuration file name.</param>
		public Xunit3TestAssembly(
			string assemblyFileName,
			string? configFileName = null)
		{
			this.assemblyFileName = Guard.ArgumentNotNullOrEmpty(nameof(assemblyFileName), assemblyFileName);
			ConfigFileName = configFileName;

			uniqueID = UniqueIDGenerator.ForAssembly(Path.GetFileNameWithoutExtension(assemblyFileName), assemblyFileName, configFileName);
		}

		/// <inheritdoc/>
		public _IAssemblyInfo Assembly => this;

		/// <inheritdoc/>
		public string? ConfigFileName { get; private set; }

		/// <inheritdoc/>
		public string UniqueID => uniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(UniqueID)} on an uninitialized '{GetType().FullName}' object");

		/// <inheritdoc/>
		public Version Version => version;

		// _IAssemblyInfo explicit implementation

		string _IAssemblyInfo.AssemblyPath => assemblyFileName ?? throw new InvalidOperationException($"Attempted to get {nameof(_IAssemblyInfo)}.{nameof(_IAssemblyInfo.AssemblyPath)} on an uninitialized '{GetType().FullName}' object");

		string _IAssemblyInfo.Name => Path.GetFileNameWithoutExtension(((_IAssemblyInfo)this).AssemblyPath) ?? "<unknown assembly>";

		IEnumerable<_IAttributeInfo> _IAssemblyInfo.GetCustomAttributes(string assemblyQualifiedAttributeTypeName) => Enumerable.Empty<_IAttributeInfo>();

		_ITypeInfo? _IAssemblyInfo.GetType(string typeName) => null;

		IEnumerable<_ITypeInfo> _IAssemblyInfo.GetTypes(bool includePrivateTypes) => Enumerable.Empty<_ITypeInfo>();
	}
}
