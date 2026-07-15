using System.Reflection;
using System.Runtime.Versioning;
using Xunit.Sdk;

namespace Xunit.v3;

internal sealed class CodeGenTestAssemblyRegistration()
{
	readonly object factoryLock = new();
	ICodeGenTestAssembly? testAssembly;

	public Assembly? Assembly =>
		testAssembly?.Assembly;

	public Dictionary<Type, FixtureFactory> AssemblyFixtureFactories { get; } = [];

	public Dictionary<string, CodeGenTestCollectionRegistration> CollectionDefinitionsByName { get; } = [];

	public Dictionary<string, CodeGenTestCollectionRegistration> CollectionDefinitionsByType { get; } = [];

	public ParallelAlgorithm? ParallelAlgorithm { get; set; }

	public int? ParallelMaxThreads { get; set; }

	public ParallelMode? ParallelMode { get; set; }

	public Func<ITestCaseOrderer>? TestCaseOrdererFactory { get; set; }

	public Func<ITestClassOrderer>? TestClassOrdererFactory { get; set; }

	public Func<ICodeGenTestAssembly, ICodeGenTestCollectionFactory> TestCollectionFactoryFactory { get; set; } =
		(assembly) => new CollectionPerClassTestCollectionFactory(assembly);

	public Func<ITestCollectionOrderer>? TestCollectionOrdererFactory { get; set; }

	public Func<ITestMethodOrderer>? TestMethodOrdererFactory { get; set; }

	public Func<string?, ITestFramework> TestFrameworkFactory { get; set; } =
		configFile => new CodeGenTestFramework(configFile);

	public Func<ITestPipelineStartup>? TestPipelineStartupFactory { get; set; }

	public Dictionary<string, HashSet<string>> Traits { get; } = new(StringComparer.Ordinal);

	public ICodeGenTestAssembly GetTestAssembly(
		Assembly assembly,
		string? configFile)
	{
		lock (factoryLock)
			if (testAssembly is null)
			{
				var name = assembly.GetName();
				var assemblyName = name.Name ?? throw new ArgumentNullException("assembly.GetName().Name", "Assembly must have a name");
				var assemblyPath = Path.Combine(AppContext.BaseDirectory, assemblyName + ".dll");
				if (!File.Exists(assemblyPath))
					assemblyPath = Path.Combine(AppContext.BaseDirectory, assemblyName + CodeGenHelper.ExecutableExtension);

				var parallelizationAttribute = new ParallelizationAttribute();
				if (ParallelAlgorithm.HasValue)
					parallelizationAttribute.Algorithm = ParallelAlgorithm.Value;
				if (ParallelMaxThreads.HasValue)
					parallelizationAttribute.MaxThreads = ParallelMaxThreads.Value;
				if (ParallelMode.HasValue)
					parallelizationAttribute.Mode = ParallelMode.Value;

				testAssembly = new CodeGenTestAssembly(
					assembly,
					AssemblyFixtureFactories,
					assemblyName,
					assemblyPath,
					assembly.GetCustomAttributes<BeforeAfterTestAttribute>().CastOrToReadOnlyCollection(),
					CollectionDefinitionsByName,
					configFile,
					assembly.Modules.FirstOrDefault()?.ModuleVersionId,
					parallelizationAttribute,
					assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName,
					Traits.ToReadOnly(),
					version: name.Version
				);
			}

		if (testAssembly.Assembly != assembly)
			throw new ArgumentException("Code generation only supports a single test assembly");

		return testAssembly;
	}
}
