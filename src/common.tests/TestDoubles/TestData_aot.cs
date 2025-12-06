#if XUNIT_AOT

using System;
using System.IO;
using System.Reflection;
using Xunit.Internal;
using Xunit.Runner.Common;

partial class TestData
{
	public static XunitProjectAssembly XunitProjectAssembly<TTestClass>(
		XunitProject? project = null,
		int xUnitVersion = 3)
	{
		var assemblySimpleName = Guard.NotNull("Assembly.GetEntryAssembly().GetName().Name returned null", Assembly.GetEntryAssembly()?.GetName().Name) + ".dll";
		var assemblyFileName = Path.Combine(AppContext.BaseDirectory, assemblySimpleName);
#if NET8_0
		var targetFramework = ".NETCoreApp,Version=v8.0";
#elif NET9_0
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

#endif  // XUNIT_AOT
