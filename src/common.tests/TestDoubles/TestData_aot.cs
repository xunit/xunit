using System.Reflection;
using Xunit.Runner.Common;
using Xunit.v3;

partial class TestData
{
	public static ValueTask<CoreTestClassCreationResult> DefaultClassFactory(FixtureMappingManager fixtureMappingManager) =>
		new(new CoreTestClassCreationResult(null));

	public static readonly IReadOnlyDictionary<string, CodeGenTestCollectionRegistration> EmptyCollectionDefinitions = new Dictionary<string, CodeGenTestCollectionRegistration>();
	public static readonly IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> EmptyFixtureFactories = new Dictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>>();

	public static XunitProjectAssembly XunitProjectAssembly<TTestClass>(
		XunitProject? project = null,
		int xUnitVersion = 3)
	{
		var assemblySimpleName = Guard.NotNull("Assembly.GetEntryAssembly().GetName().Name returned null", Assembly.GetEntryAssembly()?.GetName().Name) + ".dll";
		var assemblyFileName = Path.Combine(AppContext.BaseDirectory, assemblySimpleName).FindTestAssembly();
#if NET9_0
		var targetFramework = ".NETCoreApp,Version=v9.0";
#elif NET10_0
		var targetFramework = ".NETCoreApp,Version=v10.0";
#else
#error Unknown target framework
#endif

		var assemblyMetadata = new AssemblyMetadata(xUnitVersion, targetFramework);
		return new(project ?? new XunitProject(), assemblyFileName, assemblyMetadata);
	}
}
