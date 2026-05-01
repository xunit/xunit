using NSubstitute;
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

	// Uses type activator, but only in reflection-mode
	[Fact]
	public static async ValueTask UsesTypeActivator()
	{
		var expected = new object();
		var typeActivator = Substitute.For<ITypeActivator, InterfaceProxy<ITypeActivator>>();
		typeActivator.CreateInstance(null!, null).ReturnsForAnyArgs(expected);
		var manager = new TestableFixtureMappingManager(typeActivator);

		await manager.InitializeAsync(typeof(object));

		var cachedItem = Assert.Single(manager.GetFixtureCache());
		Assert.Equal(typeof(object), cachedItem.Key);
		Assert.Same(expected, cachedItem.Value);
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
		public TestableFixtureMappingManager(ITypeActivator typeActivator) :
			base(typeActivator, "Testable")
		{ }

		public TestableFixtureMappingManager(FixtureMappingManager parent) :
			base("Testable", parent)
		{ }

		public TestableFixtureMappingManager(params object[] cachedFixtureValues) :
			base("Testable", cachedFixtureValues)
		{ }
	}
}
