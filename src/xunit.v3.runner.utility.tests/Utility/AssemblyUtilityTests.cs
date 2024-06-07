using System;
using Xunit;
using Xunit.Runner.Common;

public class AssemblyUtilityTests
{
	[Fact]
	public void GetAssemblyMetadata()
	{
		var metadata = AssemblyUtility.GetAssemblyMetadata(typeof(AssemblyUtilityTests).Assembly.Location);

		Assert.NotNull(metadata);
		Assert.Equal(3, metadata.XunitVersion);
#if NET472
		Assert.Equal(TargetFrameworkIdentifier.DotNetFramework, metadata.TargetFrameworkIdentifier);
		Assert.Equal(new Version(4, 7, 2), metadata.TargetFrameworkVersion);
#elif NET6_0
		Assert.Equal(TargetFrameworkIdentifier.DotNetCore, metadata.TargetFrameworkIdentifier);
		Assert.Equal(new Version(6, 0), metadata.TargetFrameworkVersion);
#else
#error Unknown target framework
#endif
	}
}
