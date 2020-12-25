using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// The default implementation of <see cref="_ITestAssembly"/>.
	/// </summary>
	[DebuggerDisplay(@"\{ assembly = {Assembly.AssemblyPath}, config = {ConfigFileName} \}")]
	public class TestAssembly : _ITestAssembly, IXunitSerializable
	{
		_IAssemblyInfo? assembly;
		string? uniqueID;
		Version? version;

		/// <summary/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
		public TestAssembly()
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="TestAssembly"/> class.
		/// </summary>
		/// <param name="assembly">The test assembly.</param>
		/// <param name="configFileName">The optional configuration filename</param>
		/// <param name="version">The version number of the assembly (defaults to "0.0.0.0")</param>
		/// <param name="uniqueID">The unique ID for the test assembly (only used to override default behavior in testing scenarios)</param>
		public TestAssembly(
			_IAssemblyInfo assembly,
			string? configFileName = null,
			Version? version = null,
			string? uniqueID = null)
		{
			Assembly = Guard.ArgumentNotNull(nameof(assembly), assembly);
			ConfigFileName = configFileName;

			this.uniqueID = uniqueID ?? UniqueIDGenerator.ForAssembly(assembly.Name, assembly.AssemblyPath, configFileName);
			this.version =
				version
				?? (assembly as _IReflectionAssemblyInfo)?.Assembly?.GetName()?.Version
				?? new Version(0, 0, 0, 0);
		}

		/// <inheritdoc/>
		public _IAssemblyInfo Assembly
		{
			get => assembly ?? throw new InvalidOperationException($"Attempted to get {nameof(Assembly)} on an uninitialized '{GetType().FullName}' object");
			set => assembly = Guard.ArgumentNotNull(nameof(Assembly), value);
		}

		/// <inheritdoc/>
		public string? ConfigFileName { get; set; }

		/// <inheritdoc/>
		public string UniqueID => uniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(UniqueID)} on an uninitialized '{GetType().FullName}' object");

		/// <summary>
		/// Gets the assembly version.
		/// </summary>
		public Version Version => version ?? throw new InvalidOperationException($"Attempted to get {nameof(Version)} on an uninitialized '{GetType().FullName}' object");

		/// <inheritdoc/>
		public void Serialize(IXunitSerializationInfo info)
		{
			Guard.ArgumentNotNull(nameof(info), info);

			info.AddValue("AssemblyPath", Assembly.AssemblyPath);
			info.AddValue("ConfigFileName", ConfigFileName);
			info.AddValue("Version", Version.ToString());
		}

		/// <inheritdoc/>
		public void Deserialize(IXunitSerializationInfo info)
		{
			Guard.ArgumentNotNull(nameof(info), info);

			version = new Version(info.GetValue<string>("Version"));
			ConfigFileName = info.GetValue<string>("ConfigFileName");

			var assemblyPath = info.GetValue<string>("AssemblyPath");
			var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
			var assembly = System.Reflection.Assembly.Load(new AssemblyName
			{
				Name = assemblyName,
				Version = Version
			});

			Assembly = Reflector.Wrap(assembly);

			uniqueID = UniqueIDGenerator.ForAssembly(assemblyName, assemblyPath, ConfigFileName);
		}
	}
}
