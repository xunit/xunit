using Xunit;
using Xunit.Runner.Common;

public class AssemblyUtilityTests
{
	[Fact]
	public void GetAssemblyMetadata()
	{
#if XUNIT_AOT
		var assemblyFileName = Path.Combine(AppContext.BaseDirectory, typeof(AssemblyUtilityTests).Assembly.GetName().Name + ".dll").FindTestAssembly();
#else
		var assemblyFileName = typeof(AssemblyUtilityTests).Assembly.Location;
#endif
		var metadata = AssemblyUtility.GetAssemblyMetadata(assemblyFileName);

		Assert.NotNull(metadata);
		Assert.Equal(3, metadata.XunitVersion);
#if XUNIT_AOT
		Assert.Equal(TargetFrameworkIdentifier.UnknownTargetFramework, metadata.TargetFrameworkIdentifier);
		Assert.Equal(new Version(0, 0), metadata.TargetFrameworkVersion);
#elif NET472
		Assert.Equal(TargetFrameworkIdentifier.DotNetFramework, metadata.TargetFrameworkIdentifier);
		Assert.Equal(new Version(4, 7, 2), metadata.TargetFrameworkVersion);
#elif NET8_0
		Assert.Equal(TargetFrameworkIdentifier.DotNetCore, metadata.TargetFrameworkIdentifier);
		Assert.Equal(new Version(8, 0), metadata.TargetFrameworkVersion);
#else
#error Unknown target framework
#endif
	}
}
