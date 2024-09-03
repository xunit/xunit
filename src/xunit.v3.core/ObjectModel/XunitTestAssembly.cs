using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The default implementation of <see cref="IXunitTestAssembly"/> for xUnit.net v3.
/// </summary>
[DebuggerDisplay(@"\{ assembly = {AssemblyPath}, config = {ConfigFilePath}, id = {UniqueID} \}")]
public class XunitTestAssembly : IXunitTestAssembly, IXunitSerializable
{
	// Values that must be round-tripped in serialization
	Assembly? assembly;
	string? uniqueID;
	Version? version;

	// Lazy accessors based on serialized values
	readonly Lazy<IReadOnlyCollection<Type>> assemblyFixtureTypes;
	readonly Lazy<string> assemblyName;
	readonly Lazy<IReadOnlyCollection<IBeforeAfterTestAttribute>> beforeAfterTestAttributes;
	readonly Lazy<ICollectionBehaviorAttribute?> collectionBehavior;
	readonly Lazy<IReadOnlyDictionary<string, (Type Type, CollectionDefinitionAttribute Attribute)>> collectionDefinitions;
	readonly Lazy<string> targetFramework;
	readonly Lazy<ITestCaseOrderer?> testCaseOrderer;
	readonly Lazy<ITestCollectionOrderer?> testCollectionOrderer;
	readonly Lazy<IReadOnlyDictionary<string, IReadOnlyCollection<string>>> traits;

	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public XunitTestAssembly()
	{
		assemblyFixtureTypes = new(() => ExtensibilityPointFactory.GetAssemblyFixtureTypes(Assembly));
		assemblyName = new(() => Assembly.GetName().FullName);
		beforeAfterTestAttributes = new(() => ExtensibilityPointFactory.GetAssemblyBeforeAfterTestAttributes(Assembly));
		collectionBehavior = new(() => ExtensibilityPointFactory.GetCollectionBehavior(Assembly));
		collectionDefinitions = new(() => ExtensibilityPointFactory.GetCollectionDefinitions(Assembly));
#pragma warning disable IDE0200 // The lambda is necessary to prevent prematurely dereferencing the Assembly properly
		targetFramework = new(() => Assembly.GetTargetFramework());
#pragma warning restore IDE0200
		testCaseOrderer = new(() => ExtensibilityPointFactory.GetAssemblyTestCaseOrderer(Assembly));
		testCollectionOrderer = new(() => ExtensibilityPointFactory.GetAssemblyTestCollectionOrderer(Assembly));
		traits = new(() => ExtensibilityPointFactory.GetAssemblyTraits(Assembly));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestAssembly"/> class.
	/// </summary>
	/// <param name="assembly">The test assembly.</param>
	/// <param name="configFileName">The optional configuration filename</param>
	/// <param name="version">The version number of the assembly (defaults to "0.0.0.0")</param>
	/// <param name="uniqueID">The unique ID for the test assembly (only used to override default behavior in testing scenarios)</param>
	public XunitTestAssembly(
		Assembly assembly,
		string? configFileName = null,
		Version? version = null,
		string? uniqueID = null)
#pragma warning disable CS0618
			: this()
#pragma warning restore CS0618
	{
		this.assembly = Guard.ArgumentNotNull(assembly);
		ConfigFilePath = configFileName;

		this.uniqueID = uniqueID ?? UniqueIDGenerator.ForAssembly(assembly.Location, configFileName);
		this.version =
			version
				?? assembly.GetName().Version
				?? new Version(0, 0, 0, 0);
	}

	/// <inheritdoc/>
	public Assembly Assembly =>
		this.ValidateNullablePropertyValue(assembly, nameof(Assembly));

	/// <inheritdoc/>
	public IReadOnlyCollection<Type> AssemblyFixtureTypes =>
		assemblyFixtureTypes.Value;

	/// <inheritdoc/>
	public string AssemblyName =>
		assemblyName.Value;

	/// <inheritdoc/>
	public string AssemblyPath =>
		Assembly.Location;

	/// <inheritdoc/>
	public IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes =>
		beforeAfterTestAttributes.Value;

	/// <inheritdoc/>
	public ICollectionBehaviorAttribute? CollectionBehavior =>
		collectionBehavior.Value;

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, (Type Type, CollectionDefinitionAttribute Attribute)> CollectionDefinitions =>
		collectionDefinitions.Value;

	/// <inheritdoc/>
	public string? ConfigFilePath { get; private set; }

	/// <inheritdoc/>
	public Guid ModuleVersionID =>
		Assembly.Modules.First().ModuleVersionId;

	/// <inheritdoc/>
	public string TargetFramework =>
		targetFramework.Value;

	/// <inheritdoc/>
	public ITestCaseOrderer? TestCaseOrderer =>
		testCaseOrderer.Value;

	/// <inheritdoc/>
	public ITestCollectionOrderer? TestCollectionOrderer =>
		testCollectionOrderer.Value;

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits =>
		traits.Value;

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

		ConfigFilePath = info.GetValue<string>("cp");

		var assemblyPath = Guard.NotNull("Could not retrieve AssemblyPath from serialization", info.GetValue<string>("ap"));
		assembly = Guard.NotNull(() => "Could not load assembly " + assemblyPath, Assembly.LoadFrom(assemblyPath));

		uniqueID = Guard.NotNull("Could not retrieve UniqueID from serialization", info.GetValue<string>("id"));
	}

	/// <inheritdoc/>
	public void Serialize(IXunitSerializationInfo info)
	{
		info.AddValue("ap", AssemblyPath);
		info.AddValue("cp", ConfigFilePath);
		info.AddValue("v", Version.ToString());
		info.AddValue("id", UniqueID);
	}
}
