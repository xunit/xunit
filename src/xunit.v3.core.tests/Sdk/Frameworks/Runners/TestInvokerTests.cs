using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestInvokerTests
{
	[Fact]
	public static async void Messages_StaticTestMethod()
	{
		var messageBus = new SpyMessageBus();
		var invoker = TestableTestInvoker.Create<NonDisposableClass>("StaticPassing", messageBus);

		await invoker.RunAsync();

		Assert.Empty(messageBus.Messages);
		Assert.True(invoker.BeforeTestMethodInvoked_Called);
		Assert.True(invoker.AfterTestMethodInvoked_Called);
	}

	[Fact]
	public static async void Messages_NonStaticTestMethod_NoDispose()
	{
		var messageBus = new SpyMessageBus();
		var invoker = TestableTestInvoker.Create<NonDisposableClass>("Passing", messageBus, "Display Name");

		await invoker.RunAsync();

		Assert.Collection(
			messageBus.Messages,
			msg => Assert.IsType<_TestClassConstructionStarting>(msg),
			msg => Assert.IsType<_TestClassConstructionFinished>(msg)
		);
	}

	[Fact]
	public static async void Messages_NonStaticTestMethod_WithDispose()
	{
		var messageBus = new SpyMessageBus();
		var invoker = TestableTestInvoker.Create<DisposableClass>("Passing", messageBus, "Display Name");

		await invoker.RunAsync();

		Assert.Collection(
			messageBus.Messages,
			msg => Assert.IsType<_TestClassConstructionStarting>(msg),
			msg => Assert.IsType<_TestClassConstructionFinished>(msg),
			msg => Assert.IsType<_TestClassDisposeStarting>(msg),
			msg => Assert.IsType<_TestClassDisposeFinished>(msg)
		);
	}

	[Fact]
	public static async void Messages_NonStaticTestMethod_WithDisposeAsync()
	{
		var messageBus = new SpyMessageBus();
		var invoker = TestableTestInvoker.Create<AsyncDisposableClass>("Passing", messageBus, "Display Name");

		await invoker.RunAsync();

		Assert.Collection(
			messageBus.Messages,
			msg => Assert.IsType<_TestClassConstructionStarting>(msg),
			msg => Assert.IsType<_TestClassConstructionFinished>(msg),
			msg => Assert.IsType<_TestClassDisposeStarting>(msg),
			msg => Assert.IsType<_TestClassDisposeFinished>(msg)
		);
	}

	[Fact]
	public static async void Messages_NonStaticTestMethod_WithDisposeAndDisposeAsync()
	{
		var messageBus = new SpyMessageBus();
		var invoker = TestableTestInvoker.Create<BothDisposableClass>("Passing", messageBus, "Display Name");

		await invoker.RunAsync();

		Assert.Collection(
			messageBus.Messages,
			msg => Assert.IsType<_TestClassConstructionStarting>(msg),
			msg => Assert.IsType<_TestClassConstructionFinished>(msg),
			msg => Assert.IsType<_TestClassDisposeStarting>(msg),
			msg => Assert.IsType<_TestClassDisposeFinished>(msg)
		);
	}

	[Fact]
	public static async void Passing()
	{
		var messageBus = new SpyMessageBus();
		var invoker = TestableTestInvoker.Create<NonDisposableClass>("Passing", messageBus);

		var result = await invoker.RunAsync();

		Assert.NotEqual(0m, result);
		Assert.Null(invoker.Aggregator.ToException());
	}

	[Fact]
	public static async void Failing()
	{
		var messageBus = new SpyMessageBus();
		var invoker = TestableTestInvoker.Create<NonDisposableClass>("Failing", messageBus);

		var result = await invoker.RunAsync();

		Assert.NotEqual(0m, result);
		Assert.IsType<TrueException>(invoker.Aggregator.ToException());
	}

	[Fact]
	public static async void TooManyParameterValues()
	{
		var messageBus = new SpyMessageBus();
		var invoker = TestableTestInvoker.Create<NonDisposableClass>("Passing", messageBus, testMethodArguments: new object[] { 42 });

		await invoker.RunAsync();

		var ex = Assert.IsType<InvalidOperationException>(invoker.Aggregator.ToException());
		Assert.Equal("The test method expected 0 parameter values, but 1 parameter value was provided.", ex.Message);
	}

	[Fact]
	public static async void NotEnoughParameterValues()
	{
		var messageBus = new SpyMessageBus();
		var invoker = TestableTestInvoker.Create<NonDisposableClass>("FactWithParameter", messageBus);

		await invoker.RunAsync();

		var ex = Assert.IsType<InvalidOperationException>(invoker.Aggregator.ToException());
		Assert.Equal("The test method expected 1 parameter value, but 0 parameter values were provided.", ex.Message);
	}

	[Fact]
	public static async void CancellationRequested_DoesNotInvokeTestMethod()
	{
		var messageBus = new SpyMessageBus();
		var invoker = TestableTestInvoker.Create<NonDisposableClass>("Failing", messageBus);
		invoker.TokenSource.Cancel();

		var result = await invoker.RunAsync();

		Assert.Equal(0m, result);
		Assert.Null(invoker.Aggregator.ToException());
		Assert.False(invoker.BeforeTestMethodInvoked_Called);
		Assert.False(invoker.AfterTestMethodInvoked_Called);
	}

	[Fact]
	public static async void CancellationRequested_DisposeCalledIfClassConstructed()
	{
		var classConstructed = false;

		bool cancelThunk(_MessageSinkMessage msg)
		{
			if (msg is _TestClassConstructionFinished)
				classConstructed = true;
			return !classConstructed;
		}

		var messageBus = new SpyMessageBus(cancelThunk);
		var invoker = TestableTestInvoker.Create<DisposableClass>("Passing", messageBus, "Display Name");

		await invoker.RunAsync();

		Assert.Collection(
			messageBus.Messages,
			msg => Assert.IsType<_TestClassConstructionStarting>(msg),
			msg => Assert.IsType<_TestClassConstructionFinished>(msg),
			msg => Assert.IsType<_TestClassDisposeStarting>(msg),
			msg => Assert.IsType<_TestClassDisposeFinished>(msg)
		);
	}


	class NonDisposableClass
	{
		[Fact]
		public static void StaticPassing() { }

		[Fact]
		public void Passing() { }

		[Fact]
		public void Failing()
		{
			Assert.True(false);
		}

		[Fact]
		public void FactWithParameter(int x) { }
	}

	class DisposableClass : IDisposable
	{
		public void Dispose() { }

		[Fact]
		public void Passing() { }
	}

	class AsyncDisposableClass : IAsyncDisposable
	{
		public ValueTask DisposeAsync() => default;

		[Fact]
		public void Passing() { }
	}

	class BothDisposableClass : IAsyncDisposable, IDisposable
	{
		public void Dispose() { }

		public ValueTask DisposeAsync() => default;

		[Fact]
		public void Passing() { }
	}

	class TestableTestInvoker : TestInvoker<_ITestCase>
	{
		public readonly new ExceptionAggregator Aggregator;
		public bool AfterTestMethodInvoked_Called;
		public bool BeforeTestMethodInvoked_Called;
		public readonly new _ITestCase TestCase;
		public readonly CancellationTokenSource TokenSource;

		TestableTestInvoker(
			_ITest test,
			IMessageBus messageBus,
			Type testClass,
			MethodInfo testMethod,
			object?[]? testMethodArguments,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource) :
				base(test, messageBus, testClass, new object[0], testMethod, testMethodArguments, aggregator, cancellationTokenSource)
		{
			TestCase = test.TestCase;
			Aggregator = aggregator;
			TokenSource = cancellationTokenSource;
		}

		public static TestableTestInvoker Create<TClassUnderTest>(
			string methodName,
			IMessageBus messageBus,
			string displayName = "MockDisplayName",
			object?[]? testMethodArguments = null)
		{
			var testCase = Mocks.TestCase<TClassUnderTest>(methodName);
			var test = Mocks.Test(testCase, displayName, "test-id");

			return new TestableTestInvoker(
				test,
				messageBus,
				typeof(TClassUnderTest),
				typeof(TClassUnderTest).GetMethod(methodName) ?? throw new ArgumentException($"Could not find method '{methodName}' in '{typeof(TClassUnderTest).FullName}'"),
				testMethodArguments,
				new ExceptionAggregator(),
				new CancellationTokenSource()
			);
		}

		protected override Task AfterTestMethodInvokedAsync()
		{
			AfterTestMethodInvoked_Called = true;
			return Task.CompletedTask;
		}

		protected override Task BeforeTestMethodInvokedAsync()
		{
			BeforeTestMethodInvoked_Called = true;
			return Task.CompletedTask;
		}
	}
}
