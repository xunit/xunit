using System;
using Xunit;
using Xunit.Runner.Common;

public class TestAssemblyInfoTests
{
	[Fact]
	public void CanRoundTripSerialize()
	{
		var expected = new TestAssemblyInfo(new Version(1, 2, 3), "core-framework-informational", "target-framework", "test-framework");

		var json = expected.ToJson();
		var actual = TestAssemblyInfo.FromJson(json);

		Assert.Equivalent(expected, actual);
	}
}
