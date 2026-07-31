using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Default implementation of <see cref="IXunitTestAssembly"/> for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
[DebuggerDisplay(@"\{ assembly = {AssemblyPath}, config = {ConfigFilePath}, id = {UniqueID} \}")]
public class XunitTestAssembly : IXunitTestAssembly, IXunitSerializable
{
	// Values that must be round-tripped in serialization
	Assembly? assembly;
	string? assemblyName;
	string? assemblyPath;
	string? targetFramework;
	string? uniqueID;
	Version? version;

	// Lazy accessors based on expensive-to-compute values
	readonly Lazy<IReadOnlyCollection<Type>> lazyAssemblyFixtureTypes;
	readonly Lazy<string> lazyAssemblyName;
	readonly Lazy<string> lazyAssemblyPath;
	readonly Lazy<IReadOnlyCollection<IBeforeAfterTestAttribute>> lazyBeforeAfterTestAttributes;
	readonly Lazy<ICollectionBehaviorAttribute?> lazyCollectionBehavior;
	readonly Lazy<IReadOnlyDictionary<string, (Type Type, CollectionDefinitionAttribute Attribute)>> lazyCollectionDefinitions;
	readonly Lazy<Guid> lazyModuleVersionID;
	readonly Lazy<IParallelizationAttribute?> lazyParallelization;
	readonly Lazy<string> lazyTargetFramework;
	readonly Lazy<ITestCaseOrderer?> lazyTestCaseOrderer;
	readonly Lazy<ITestClassOrderer?> lazyTestClassOrderer;
	readonly Lazy<ITestCollectionOrderer?> lazyTestCollectionOrderer;
	readonly Lazy<ITestMethodOrderer?> lazyTestMethodOrderer;
	readonly Lazy<IReadOnlyDictionary<string, IReadOnlyCollection<string>>> lazyTraits;
	readonly Lazy<string> lazyUniqueID;
	readonly Lazy<Version> lazyVersion;

	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public XunitTestAssembly()
	{
		lazyAssemblyFixtureTypes = new(() => ExtensibilityPointFactory.GetAssemblyFixtureTypes(Assembly));
		lazyAssemblyName = new(() => Assembly.GetName().FullName);
		lazyAssemblyPath = new(() => Assembly.Location);
		lazyBeforeAfterTestAttributes = new(() => ExtensibilityPointFactory.GetAssemblyBeforeAfterTestAttributes(Assembly));
		lazyCollectionBehavior = new(() => ExtensibilityPointFactory.GetCollectionBehavior(Assembly));
		lazyCollectionDefinitions = new(() => ExtensibilityPointFactory.GetCollectionDefinitions(Assembly));
		lazyModuleVersionID = new(() => Assembly.Modules.FirstOrDefault()?.ModuleVersionId ?? Guid.Empty);
		lazyParallelization = new(() => ExtensibilityPointFactory.GetAssemblyParallelization(Assembly));
		lazyTargetFramework = new(() => Assembly.GetTargetFramework());
		lazyTestCaseOrderer = new(() => RegisteredEngineConfig.GetAssemblyTestCaseOrderer(Assembly));
		lazyTestClassOrderer = new(() => RegisteredEngineConfig.GetAssemblyTestClassOrderer(Assembly));
		lazyTestCollectionOrderer = new(() => RegisteredEngineConfig.GetAssemblyTestCollectionOrderer(Assembly));
		lazyTestMethodOrderer = new(() => RegisteredEngineConfig.GetAssemblyTestMethodOrderer(Assembly));
		lazyTraits = new(() => ExtensibilityPointFactory.GetAssemblyTraits(Assembly));
		lazyUniqueID = new(() => UniqueIDGenerator.ForAssembly(AssemblyPath, ConfigFilePath));
		lazyVersion = new(() => Assembly.GetName().Version ?? new Version(0, 0, 0, 0));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestAssembly"/> class.
	/// </summary>
	/// <param name="assembly">The test assembly</param>
	/// <param name="configFilePath">The configuration file path (pass <see langword="null"/> for no configuration file)</param>
	/// <param name="assemblyName">The optional assembly name (defaults to <c><see cref="Assembly.GetName()"/>.FullName</c>)</param>
	/// <param name="assemblyPath">The optional assembly path (defaults to <c><see cref="Assembly.Location"/></c>)</param>
	/// <param name="targetFramework">The optional target framework (defaults to value from <see cref="TargetFrameworkAttribute"/>)</param>
	/// <param name="uniqueID">The optional unique ID (defaults to <c><see cref="UniqueIDGenerator.ForAssembly"/></c>)</param>
	/// <param name="version">The optional version (defaults to <c><see cref="Assembly.GetName()"/>.Version</c>)</param>
	public XunitTestAssembly(
		Assembly assembly,
		string? configFilePath,
		string? assemblyName = null,
		string? assemblyPath = null,
		string? targetFramework = null,
		string? uniqueID = null,
		Version? version = null)
#pragma warning disable CS0618
			: this()
#pragma warning restore CS0618
	{
		this.assembly = Guard.ArgumentNotNull(assembly);
		ConfigFilePath = configFilePath;
		this.assemblyName = assemblyName;
		this.assemblyPath = assemblyPath;
		this.targetFramework = targetFramework;
		this.uniqueID = uniqueID;
		this.version = version;
	}

	/// <summary>
	/// Please call <see cref="XunitTestAssembly(Assembly, string?, string?, string?, string?, string?, Version?)"/> instead.
	/// This overload will be removed in the next major version.
	/// </summary>
	[Obsolete("Please call the new overload with additional parameters. This overload will be removed in the next major version.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[OverloadResolutionPriority(-1)]
	public XunitTestAssembly(
		Assembly assembly,
		string? configFileName = null,
		Version? version = null,
		string? uniqueID = null)
			: this(assembly, configFileName, uniqueID: uniqueID, version: version)
	{ }

	/// <inheritdoc/>
	public Assembly Assembly =>
		this.ValidateNullablePropertyValue(assembly, nameof(Assembly));

	/// <inheritdoc/>
	public IReadOnlyCollection<Type> AssemblyFixtureTypes =>
		lazyAssemblyFixtureTypes.Value;

	/// <inheritdoc/>
	public string AssemblyName =>
		assemblyName ?? lazyAssemblyName.Value;

	/// <inheritdoc/>
	public string AssemblyPath =>
		assemblyPath ?? lazyAssemblyPath.Value;

	/// <inheritdoc/>
	public IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes =>
		lazyBeforeAfterTestAttributes.Value;

	/// <inheritdoc/>
	public ICollectionBehaviorAttribute? CollectionBehavior =>
		lazyCollectionBehavior.Value;

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, (Type Type, CollectionDefinitionAttribute Attribute)> CollectionDefinitions =>
		lazyCollectionDefinitions.Value;

	/// <inheritdoc/>
	public string? ConfigFilePath { get; private set; }

	/// <inheritdoc/>
	public int? MaxParallelThreads =>
		lazyParallelization.Value?.GetMaxThreads();

	/// <inheritdoc/>
	public ParallelAlgorithm? ParallelAlgorithm =>
		lazyParallelization.Value?.GetAlgorithm();

	/// <inheritdoc/>
	public ParallelMode? ParallelMode =>
		lazyParallelization.Value?.GetMode();

	/// <inheritdoc/>
	public Guid ModuleVersionID =>
		lazyModuleVersionID.Value;

	/// <inheritdoc/>
	public string TargetFramework =>
		targetFramework ?? lazyTargetFramework.Value;

	/// <inheritdoc/>
	public ITestCaseOrderer? TestCaseOrderer =>
		lazyTestCaseOrderer.Value;

	/// <inheritdoc/>
	public ITestClassOrderer? TestClassOrderer =>
		lazyTestClassOrderer.Value;

	/// <inheritdoc/>
	public ITestCollectionOrderer? TestCollectionOrderer =>
		lazyTestCollectionOrderer.Value;

	/// <inheritdoc/>
	public ITestMethodOrderer? TestMethodOrderer =>
		lazyTestMethodOrderer.Value;

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits =>
		lazyTraits.Value;

	/// <inheritdoc/>
	public string UniqueID =>
		uniqueID ?? lazyUniqueID.Value;

	/// <inheritdoc/>
	public Version Version =>
		version ?? lazyVersion.Value;

	/// <inheritdoc/>
	public void Deserialize(IXunitSerializationInfo info)
	{
		// AssemblyPath is always required, so we can load the assembly
		assemblyPath = Guard.NotNull("Could not retrieve AssemblyPath from serialization", info.GetValue<string>("ap"));
		assembly = Guard.NotNull(() => "Could not load assembly " + assemblyPath, Assembly.LoadFrom(assemblyPath));

		// Everything else is optional
		assemblyName = info.GetValue<string>("an") ?? assemblyName;
		ConfigFilePath = info.GetValue<string>("cp") ?? ConfigFilePath;
		targetFramework = info.GetValue<string>("tf") ?? targetFramework;
		uniqueID = info.GetValue<string>("id") ?? uniqueID;

		if (info.GetValue<string>("v") is { } versionString)
			version = new Version(versionString);
	}

	/// <inheritdoc/>
	public void Serialize(IXunitSerializationInfo info)
	{
		// Always serialize the path so we can load the assembly during deserialization
		info.AddValue("ap", AssemblyPath);

		// For the rest, only serialize when the user has provided an override
		if (assemblyName is not null)
			info.AddValue("an", assemblyName);
		if (ConfigFilePath is not null)
			info.AddValue("cp", ConfigFilePath);
		if (targetFramework is not null)
			info.AddValue("tf", targetFramework);
		if (uniqueID is not null)
			info.AddValue("id", uniqueID);
		if (version is not null)
			info.AddValue("v", version.ToString());
	}
}
