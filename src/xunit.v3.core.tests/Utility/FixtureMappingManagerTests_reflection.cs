using Xunit;
using Xunit.Sdk;
using Xunit.v3;

partial class FixtureMappingManagerTests
{
	// Native AOT skips these in the generator
	[Fact]
	public static async ValueTask MoreThanOneConstructorThrows()
	{
		var manager = new TestableFixtureMappingManager();

		var ex = await Record.ExceptionAsync(async () => await manager.InitializeAsync(typeof(int)));

		Assert.IsType<TestPipelineException>(ex);
		Assert.Equal("Testable fixture type 'System.Int32' may only define a single public constructor.", ex.Message);
	}

	// The source generated fixture factory makes the creation decision in Native AOT
	[Fact]
	public static async ValueTask WithCreateInstancesFalse_WillNotPreCreateByDefault()
	{
		var manager = new TestableFixtureMappingManager();

		await manager.InitializeAsync([typeof(object)], createInstances: false);

		Assert.Empty(manager.GetFixtureCache());
	}

	[Fact]
	public static async ValueTask WithCreateInstancesFalse_WillPreCreateInstancesWithMarkerInterface()
	{
		var manager = new TestableFixtureMappingManager();

		await manager.InitializeAsync([typeof(FixtureWithNotifyTestLifecycle)], createInstances: false);

		var cached = Assert.Single(manager.GetFixtureCache());
		Assert.Equal(typeof(FixtureWithNotifyTestLifecycle), cached.Key);
		Assert.IsType<FixtureWithNotifyTestLifecycle>(cached.Value);
	}

	class FixtureWithNotifyTestLifecycle : INotifyLifecycle { }

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
