using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class FixtureMappingManagerTests
{
	[Fact]
	public async ValueTask ReturnsNullForUnregisteredType()
	{
		var manager = new TestableFixtureMappingManager();

		var result = await manager.GetFixture(typeof(int));

		Assert.Null(result);
	}

	[Fact]
	public async ValueTask ReturnsCachedValue()
	{
		var manager = new TestableFixtureMappingManager(12);

		var result = await manager.GetFixture(typeof(int));

		Assert.Equal(12, result);
	}

	[Fact]
	public async ValueTask ReturnsValueFromParentWhenNotPresentLocally()
	{
		var parent = new TestableFixtureMappingManager(12);
		var manager = new TestableFixtureMappingManager(parent);

		var result = await manager.GetFixture(typeof(int));

		Assert.Equal(12, result);
	}

	[Fact]
	public async ValueTask ConstructsRegisteredType()
	{
		var manager = new TestableFixtureMappingManager();
		await manager.InitializeAsync(typeof(object));

		var result = await manager.GetFixture(typeof(object));

		Assert.IsType<object>(result);
	}

	[Fact]
	public async ValueTask FixtureWithThrowingCtorThrows()
	{
		var manager = new TestableFixtureMappingManager();

		var ex = await Record.ExceptionAsync(() => manager.InitializeAsync(typeof(FixtureWithThrowingCtor)));

		Assert.IsType<TestPipelineException>(ex);
		Assert.Equal("Testable fixture type 'FixtureMappingManagerTests+FixtureWithThrowingCtor' threw in its constructor", ex.Message);
		Assert.IsType<DivideByZeroException>(ex.InnerException);
	}

	class FixtureWithThrowingCtor
	{
		public FixtureWithThrowingCtor() => throw new DivideByZeroException();
	}

	[Fact]
	public async ValueTask FixtureWithThrowingInitializeAsyncThrows()
	{
		var manager = new TestableFixtureMappingManager();

		var ex = await Record.ExceptionAsync(() => manager.InitializeAsync(typeof(FixtureWithThrowingInitializeAsync)));

		Assert.IsType<TestPipelineException>(ex);
		Assert.Equal("Testable fixture type 'FixtureMappingManagerTests+FixtureWithThrowingInitializeAsync' threw in InitializeAsync", ex.Message);
		Assert.IsType<DivideByZeroException>(ex.InnerException);
	}

	class FixtureWithThrowingInitializeAsync : IAsyncLifetime
	{
		public ValueTask DisposeAsync() => default;
		public ValueTask InitializeAsync() => throw new DivideByZeroException();
	}

	[Fact]
	public async ValueTask MissingDependencyThrows()
	{
		var manager = new TestableFixtureMappingManager();

		var ex = await Record.ExceptionAsync(() => manager.InitializeAsync(typeof(FixtureWithDependency)));

		Assert.IsType<TestPipelineException>(ex);
		Assert.Equal("Testable fixture type 'FixtureMappingManagerTests+FixtureWithDependency' had one or more unresolved constructor arguments: Object dependency", ex.Message);
	}

	class FixtureWithDependency(object dependency)
	{
		public object Dependency { get; } = dependency;
	}

	[Fact]
	public async ValueTask CanProvideParentCtorArgs()
	{
		var parent = new TestableFixtureMappingManager();
		await parent.InitializeAsync(typeof(object));
		var manager = new TestableFixtureMappingManager(parent);
		await manager.InitializeAsync(typeof(FixtureWithDependency));

		var result = await manager.GetFixture(typeof(FixtureWithDependency));

		var typedResult = Assert.IsType<FixtureWithDependency>(result);
		Assert.IsType<object>(typedResult.Dependency);
	}

	[Fact]
	public async ValueTask CanProvideMessageSinkAndTestContextAccessorCtorArgs()
	{
		var manager = new TestableFixtureMappingManager();
		await manager.InitializeAsync(typeof(FixtureWithMessageSinkAndTestContext));

		var result = await manager.GetFixture(typeof(FixtureWithMessageSinkAndTestContext));

		var typedResult = Assert.IsType<FixtureWithMessageSinkAndTestContext>(result);
		Assert.NotNull(TestContext.Current);
		Assert.Same(TestContext.Current, typedResult.ContextAccessor.Current);
		Assert.Same(TestContext.CurrentInternal.DiagnosticMessageSink, typedResult.MessageSink);
	}

	class FixtureWithMessageSinkAndTestContext(
		IMessageSink messageSink,
		ITestContextAccessor contextAccessor)
	{
		public ITestContextAccessor ContextAccessor { get; } = contextAccessor;
		public IMessageSink MessageSink { get; } = messageSink;
	}

	[Fact]
	public async ValueTask CallsDispose()
	{
		var manager = new TestableFixtureMappingManager();
		await manager.InitializeAsync(typeof(FixtureWithDispose));

		var result = await manager.GetFixture(typeof(FixtureWithDispose));

		var typedResult = Assert.IsType<FixtureWithDispose>(result);
		Assert.False(typedResult.DisposeCalled);

		await manager.DisposeAsync();

		Assert.True(typedResult.DisposeCalled);
	}

	class FixtureWithDispose : IDisposable
	{
		public bool DisposeCalled { get; set; }

		public void Dispose() => DisposeCalled = true;
	}

	[Fact]
	public async ValueTask FixtureWithThrowingDisposeThrows()
	{
		var manager = new TestableFixtureMappingManager();
		await manager.InitializeAsync(typeof(FixtureWithThrowingDispose));

		await manager.GetFixture(typeof(FixtureWithThrowingDispose));
		var ex = await Record.ExceptionAsync(manager.DisposeAsync);

		Assert.IsType<TestPipelineException>(ex);
		Assert.Equal($"Testable fixture type '{typeof(FixtureWithThrowingDispose).SafeName()}' threw in Dispose", ex.Message);
		Assert.IsType<DivideByZeroException>(ex.InnerException);
	}

	class FixtureWithThrowingDispose : IDisposable
	{
		public void Dispose() => throw new DivideByZeroException();
	}

	[Fact]
	public async ValueTask CallsDisposeAsync()
	{
		var manager = new TestableFixtureMappingManager();
		await manager.InitializeAsync(typeof(FixtureWithDisposeAsync));

		var result = await manager.GetFixture(typeof(FixtureWithDisposeAsync));

		var typedResult = Assert.IsType<FixtureWithDisposeAsync>(result);
		Assert.False(typedResult.DisposeAsyncCalled);

		await manager.DisposeAsync();

		Assert.True(typedResult.DisposeAsyncCalled);
	}

	class FixtureWithDisposeAsync : IAsyncDisposable
	{
		public bool DisposeAsyncCalled { get; set; }

		public ValueTask DisposeAsync()
		{
			DisposeAsyncCalled = true;
			return default;
		}
	}

	[Fact]
	public async ValueTask FixtureWithThrowingDisposeAsyncThrows()
	{
		var manager = new TestableFixtureMappingManager();
		await manager.InitializeAsync(typeof(FixtureWithThrowingDisposeAsync));

		await manager.GetFixture(typeof(FixtureWithThrowingDisposeAsync));
		var ex = await Record.ExceptionAsync(manager.DisposeAsync);

		Assert.IsType<TestPipelineException>(ex);
		Assert.Equal($"Testable fixture type '{typeof(FixtureWithThrowingDisposeAsync).SafeName()}' threw in DisposeAsync", ex.Message);
		Assert.IsType<DivideByZeroException>(ex.InnerException);
	}

	class FixtureWithThrowingDisposeAsync : IAsyncDisposable
	{
		public ValueTask DisposeAsync() => throw new DivideByZeroException();
	}

	[Fact]
	public async ValueTask MoreThanOneConstructorThrows()
	{
		var manager = new TestableFixtureMappingManager();

		var ex = await Record.ExceptionAsync(() => manager.InitializeAsync(typeof(int)));

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
