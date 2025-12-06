#if !XUNIT_AOT

using System;
using System.Reflection;
using System.Runtime.Versioning;
using Xunit.Runner.Common;

partial class TestData
{
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

#endif  // !XUNIT_AOT
