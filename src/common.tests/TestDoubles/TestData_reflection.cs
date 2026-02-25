using System.Reflection;
using System.Runtime.Versioning;
using Xunit;
using Xunit.Runner.Common;

partial class TestData
{
	public static readonly Dictionary<string, (Type, CollectionDefinitionAttribute)> EmptyCollectionDefinitions = [];

	public static XunitProjectAssembly XunitProjectAssembly<TTestClass>(
		XunitProject? project = null,
		int xUnitVersion = 3)
	{
		var assemblyFileName = typeof(TTestClass).Assembly.Location;
		var targetFramework =
			typeof(TTestClass).Assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName
				?? throw new InvalidOperationException($"Assembly '{assemblyFileName}' does not have an assembly-level TargetFrameworkAttribute");

		var assemblyMetadata = new AssemblyMetadata(xUnitVersion, targetFramework);
		return new(project ?? new XunitProject(), assemblyFileName, assemblyMetadata);
	}
}
