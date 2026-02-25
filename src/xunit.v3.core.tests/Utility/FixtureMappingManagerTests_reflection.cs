using Xunit;
using Xunit.Sdk;
using Xunit.v3;

partial class FixtureMappingManagerTests
{
	// Native AOT reports these in the generator
	[Fact]
	public async ValueTask MoreThanOneConstructorThrows()
	{
		var manager = new TestableFixtureMappingManager();

		var ex = await Record.ExceptionAsync(async () => await manager.InitializeAsync(typeof(int)));

		Assert.IsType<TestPipelineException>(ex);
		Assert.Equal("Testable fixture type 'System.Int32' may only define a single public constructor.", ex.Message);
	}

	class TestableFixtureMappingManager : FixtureMappingManager
	{
		public TestableFixtureMappingManager(FixtureMappingManager parent) :
			base("Testable", parent)
		{ }

		public TestableFixtureMappingManager(params object[] cachedFixtureValues) :
			base("Testable", cachedFixtureValues)
		{ }
	}
}
