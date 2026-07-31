using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;

public static class TestAssemblyInfoTests
{
	[Fact]
	public static void CanRoundTripSerialize()
	{
		var expected = new TestAssemblyInfo(new Version(1, 2, 3), "core-framework-informational", 42, ParallelAlgorithm.Aggressive, ParallelMode.All, "target-framework", "test-framework");

		var json = expected.ToJson();
		var actual = TestAssemblyInfo.FromJson(json);

		Assert.Equal(expected.ArchOS, actual.ArchOS);
		Assert.Equal(expected.ArchProcess, actual.ArchProcess);
		Assert.Equal(expected.CoreFramework, actual.CoreFramework);
		Assert.Equal(expected.CoreFrameworkInformational, actual.CoreFrameworkInformational);
		Assert.Equal(expected.MaxParallelThreads, actual.MaxParallelThreads);
		Assert.Equal(expected.ParallelAlgorithm, actual.ParallelAlgorithm);
		Assert.Equal(expected.ParallelMode, actual.ParallelMode);
		Assert.Equal(expected.PointerSize, actual.PointerSize);
		Assert.Equal(expected.RuntimeFramework, actual.RuntimeFramework);
		Assert.Equal(expected.TargetFramework, actual.TargetFramework);
		Assert.Equal(expected.TestFramework, actual.TestFramework);
	}
}
