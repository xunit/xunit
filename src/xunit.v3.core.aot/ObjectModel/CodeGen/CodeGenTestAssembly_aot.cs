using System.Reflection;
using System.Runtime.Versioning;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Default implementation of <see cref="ICodeGenTestAssembly"/> for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public sealed class CodeGenTestAssembly : ICodeGenTestAssembly
{
	readonly CollectionBehaviorAttribute? collectionBehavior;
	readonly Lazy<ITestCaseOrderer?> testCaseOrderer;
	readonly Lazy<ITestClassOrderer?> testClassOrderer;
	readonly Lazy<ICodeGenTestCollectionFactory> testCollectionFactory;
	readonly Lazy<ITestCollectionOrderer?> testCollectionOrderer;
	readonly Lazy<ITestMethodOrderer?> testMethodOrderer;

	/// <summary>
	/// Initializes a new instance of the <see cref="CodeGenTestAssembly"/> class.
	/// </summary>
	/// <param name="assembly">The test assembly</param>
	/// <param name="assemblyFixtureFactories">The fixture factories for assembly-level test fixtures</param>
	/// <param name="assemblyName">The assembly name</param>
	/// <param name="assemblyPath">The assembly path</param>
	/// <param name="beforeAfterTestAttributes">The <see cref="BeforeAfterTestAttribute"/>s attached to the test assembly</param>
	/// <param name="collectionBehavior">The collection behavior attribute attached to the test assembly</param>
	/// <param name="collectionDefinitions">The mapping of collection name to collection definition</param>
	/// <param name="configFilePath">The configuration file path</param>
	/// <param name="moduleVersionID">The primary module's version ID</param>
	/// <param name="targetFramework">The target framework (in form like <c>".NETCoreApp,Version=v8.0"</c>)</param>
	/// <param name="traits">The assembly-level traits</param>
	/// <param name="version">The assembly version</param>
	/// <param name="uniqueID">The optional assembly unique ID</param>
	public CodeGenTestAssembly(
		Assembly assembly,
		IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> assemblyFixtureFactories,
		string assemblyName,
		string assemblyPath,
		IReadOnlyCollection<BeforeAfterTestAttribute> beforeAfterTestAttributes,
		CollectionBehaviorAttribute? collectionBehavior,
		IReadOnlyDictionary<string, CodeGenTestCollectionRegistration> collectionDefinitions,
		string? configFilePath,
		Guid? moduleVersionID,
		string? targetFramework,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits,
		Version? version,
		string? uniqueID = null)
	{
		Assembly = Guard.ArgumentNotNull(assembly);
		AssemblyFixtureFactories = Guard.ArgumentNotNull(assemblyFixtureFactories);
		AssemblyName = Guard.ArgumentNotNull(assemblyName);
		AssemblyPath = Guard.ArgumentNotNull(assemblyPath);
		BeforeAfterTestAttributes = Guard.ArgumentNotNull(beforeAfterTestAttributes);
		this.collectionBehavior = collectionBehavior;
		CollectionDefinitions = Guard.ArgumentNotNull(collectionDefinitions);
		ConfigFilePath = configFilePath;
		ModuleVersionID = moduleVersionID ?? Guid.NewGuid();
		TargetFramework = targetFramework ?? "Unknown,Version=v0.0";
		Traits = Guard.ArgumentNotNull(traits);
		UniqueID = uniqueID ?? UniqueIDGenerator.ForAssembly(assemblyPath, configFilePath);
		Version = version ?? new Version(0, 0, 0);

		testCaseOrderer = new(() => RegisteredEngineConfig.GetAssemblyTestCaseOrderer(Assembly));
		testClassOrderer = new(() => RegisteredEngineConfig.GetAssemblyTestClassOrderer(Assembly));
		testCollectionFactory = new(() => RegisteredEngineConfig.GetTestCollectionFactory(this));
		testCollectionOrderer = new(() => RegisteredEngineConfig.GetAssemblyTestCollectionOrderer(Assembly));
		testMethodOrderer = new(() => RegisteredEngineConfig.GetAssemblyTestMethodOrderer(Assembly));
	}

	/// <inheritdoc/>
	public Assembly Assembly { get; }

	/// <inheritdoc/>
	public IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> AssemblyFixtureFactories { get; }

	/// <inheritdoc/>
	public string AssemblyName { get; }

	/// <inheritdoc/>
	public string AssemblyPath { get; }

	/// <inheritdoc/>
	public IReadOnlyCollection<BeforeAfterTestAttribute> BeforeAfterTestAttributes { get; }

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, CodeGenTestCollectionRegistration> CollectionDefinitions { get; }

	/// <inheritdoc/>
	public string? ConfigFilePath { get; }

	/// <inheritdoc/>
	public bool? DisableParallelization =>
		collectionBehavior?.DisableTestParallelization;

	/// <inheritdoc/>
	public int? MaxParallelThreads =>
		collectionBehavior?.MaxParallelThreads;

	/// <inheritdoc/>
	public ParallelAlgorithm? ParallelAlgorithm =>
		collectionBehavior?.ParallelAlgorithm;

	/// <inheritdoc/>
	public Guid ModuleVersionID { get; }

	/// <remarks>
	/// If the assembly does not have a <see cref="TargetFrameworkAttribute"/> attached to it,
	/// will return <c>"Unknown,Version=v0.0"</c>.
	/// </remarks>
	/// <inheritdoc/>
	public string TargetFramework { get; }

	/// <inheritdoc/>
	public ITestCaseOrderer? TestCaseOrderer =>
		testCaseOrderer.Value;

	/// <inheritdoc/>
	public ITestClassOrderer? TestClassOrderer =>
		testClassOrderer.Value;

	/// <summary>
	/// Gets the test collection factory for the test assembly.
	/// </summary>
	public ICodeGenTestCollectionFactory TestCollectionFactory =>
		testCollectionFactory.Value;

	/// <inheritdoc/>
	public ITestCollectionOrderer? TestCollectionOrderer =>
		testCollectionOrderer.Value;

	/// <inheritdoc/>
	public ITestMethodOrderer? TestMethodOrderer =>
		testMethodOrderer.Value;

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; }

	/// <inheritdoc/>
	public string UniqueID { get; }

	/// <remarks>
	/// If the assembly is unversioned, will return <c>0.0.0</c>.
	/// </remarks>
	/// <inheritdoc/>
	public Version Version { get; }
}
