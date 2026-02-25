using System.Reflection;
using System.Runtime.Versioning;

namespace Xunit.v3;

internal sealed class CodeGenTestAssemblyRegistration()
{
	readonly object factoryLock = new();
	ICodeGenTestAssembly? testAssembly;

	public Assembly? Assembly =>
		testAssembly?.Assembly;

	public Dictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> AssemblyFixtureFactories { get; } = [];

	public Dictionary<string, CodeGenTestCollectionRegistration> CollectionDefinitionsByName { get; } = [];

	public Dictionary<string, CodeGenTestCollectionRegistration> CollectionDefinitionsByType { get; } = [];

	public Func<ITestCaseOrderer>? TestCaseOrdererFactory { get; set; }

	public Func<ITestClassOrderer>? TestClassOrdererFactory { get; set; }

	public Func<ICodeGenTestAssembly, ICodeGenTestCollectionFactory> TestCollectionFactoryFactory { get; set; } =
		(assembly) => new CollectionPerClassTestCollectionFactory(assembly);

	public Func<ITestCollectionOrderer>? TestCollectionOrdererFactory { get; set; }

	public Func<ITestMethodOrderer>? TestMethodOrdererFactory { get; set; }

	public Func<string?, ITestFramework> TestFrameworkFactory { get; set; } =
		configFile => new CodeGenTestFramework(configFile);

	public Func<ITestPipelineStartup>? TestPipelineStartupFactory { get; set; }

	public ICodeGenTestAssembly GetTestAssembly(
		Assembly assembly,
		string? configFile)
	{
		lock (factoryLock)
			if (testAssembly is null)
			{
				var name = assembly.GetName();
				var assemblyName = name.Name ?? throw new ArgumentNullException("assembly.GetName().Name", "Assembly must have a name");

				var traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
				foreach (var traitAttribute in assembly.GetCustomAttributes<TraitAttribute>())
					traits.AddOrGet(traitAttribute.Name).Add(traitAttribute.Value);

				var assemblyPath = Path.Combine(AppContext.BaseDirectory, assemblyName + ".dll");
				if (!File.Exists(assemblyPath))
					assemblyPath = Path.Combine(AppContext.BaseDirectory, assemblyName + CodeGenHelper.ExecutableExtension);

				testAssembly = new CodeGenTestAssembly(
					assembly,
					AssemblyFixtureFactories,
					assemblyName,
					assemblyPath,
					assembly.GetCustomAttributes<BeforeAfterTestAttribute>().CastOrToReadOnlyCollection(),
					assembly.GetCustomAttribute<CollectionBehaviorAttribute>(),
					CollectionDefinitionsByName,
					configFile,
					assembly.Modules.FirstOrDefault()?.ModuleVersionId,
					assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName,
					traits.ToReadOnly(),
					version: name.Version
				);
			}

		if (testAssembly.Assembly != assembly)
			throw new ArgumentException("Code generation only supports a single test assembly");

		return testAssembly;
	}
}
