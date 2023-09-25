using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The default implementation of <see cref="_ITestAssembly"/>.
/// </summary>
[DebuggerDisplay(@"\{ assembly = {Assembly.AssemblyPath}, config = {ConfigFileName} \}")]
public class TestAssembly : _ITestAssembly, IXunitSerializable
{
	_IAssemblyInfo? assembly;
	string? uniqueID;
	Version? version;

	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
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
		this.assembly = Guard.ArgumentNotNull(assembly);
		ConfigFileName = configFileName;

		this.uniqueID = uniqueID ?? UniqueIDGenerator.ForAssembly(assembly.Name, assembly.AssemblyPath, configFileName);
		this.version =
			version
			?? (assembly as _IReflectionAssemblyInfo)?.Assembly?.GetName()?.Version
			?? new Version(0, 0, 0, 0);
	}

	/// <inheritdoc/>
	public _IAssemblyInfo Assembly =>
		this.ValidateNullablePropertyValue(assembly, nameof(Assembly));

	/// <inheritdoc/>
	public string? ConfigFileName { get; private set; }

	/// <inheritdoc/>
	public string UniqueID =>
		this.ValidateNullablePropertyValue(uniqueID, nameof(UniqueID));

	/// <inheritdoc/>
	public Version Version =>
		this.ValidateNullablePropertyValue(version, nameof(Version));

	/// <inheritdoc/>
	public void Deserialize(IXunitSerializationInfo info)
	{
		var versionString = Guard.NotNull("Could not retrieve Version from serialization", info.GetValue<string>("v"));
		version = new Version(versionString);

		ConfigFileName = info.GetValue<string>("cfn");

		var assemblyPath = Guard.NotNull("Could not retrieve AssemblyPath from serialization", info.GetValue<string>("ap"));
		var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
		var assembly = System.Reflection.Assembly.Load(new AssemblyName { Name = assemblyName, Version = Version });

		this.assembly = Reflector.Wrap(assembly);

		uniqueID = UniqueIDGenerator.ForAssembly(assemblyName, assemblyPath, ConfigFileName);
	}

	/// <inheritdoc/>
	public void Serialize(IXunitSerializationInfo info)
	{
		info.AddValue("ap", Assembly.AssemblyPath);
		info.AddValue("cfn", ConfigFileName);
		info.AddValue("v", Version.ToString());
	}
}
