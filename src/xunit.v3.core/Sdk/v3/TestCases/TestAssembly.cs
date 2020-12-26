using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// The default implementation of <see cref="_ITestAssembly"/>.
	/// </summary>
	[Serializable]
	[DebuggerDisplay(@"\{ assembly = {Assembly.AssemblyPath}, config = {ConfigFileName} \}")]
	public class TestAssembly : _ITestAssembly, ISerializable
	{
		_IAssemblyInfo? assembly;
		string? uniqueID;

		/// <summary>
		/// Used for de-serialization.
		/// </summary>
		protected TestAssembly(
			SerializationInfo info,
			StreamingContext context)
		{
			Version = Guard.NotNull("Could not retrieve Version from serialization", info.GetValue<Version>("Version"));
			ConfigFileName = info.GetValue<string>("ConfigFileName");

			var assemblyPath = Guard.NotNull("Could not retrieve AssemblyPath from serialization", info.GetValue<string>("AssemblyPath"));
			var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
			var assembly = System.Reflection.Assembly.Load(new AssemblyName
			{
				Name = assemblyName,
				Version = Version
			});

			Assembly = Reflector.Wrap(assembly);

			uniqueID = UniqueIDGenerator.ForAssembly(assemblyName, assemblyPath, ConfigFileName);
		}

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
			Version =
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
		public Version Version { get; }

		/// <inheritdoc/>
		public virtual void GetObjectData(
			SerializationInfo info,
			StreamingContext context)
		{
			info.AddValue("AssemblyPath", Assembly.AssemblyPath);
			info.AddValue("ConfigFileName", ConfigFileName);
			info.AddValue("Version", Version);
		}
	}
}
