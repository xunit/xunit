using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestInvokerTests
{
	public class Timeout
	{
		[Fact]
		public async ValueTask WithoutTimeout_RunsTest()
		{
			var invoker = new TestableXunitTestInvoker(nameof(ClassUnderTest.WithoutTimeout));

			await invoker.RunAsync();

			Assert.Null(invoker.Aggregator.ToException());
			Assert.True(invoker.TestClassInstance.WithoutTimeout_Called);
			Assert.False(invoker.CancellationTokenSource.IsCancellationRequested);
		}

		[Fact]
		public async ValueTask WithTimeout_NonAsyncMethod_Fails()
		{
			var invoker = new TestableXunitTestInvoker(nameof(ClassUnderTest.TimeoutWithoutAsync), timeout: 10);

			await invoker.RunAsync();

			var ex = invoker.Aggregator.ToException();
			Assert.IsType<TestTimeoutException>(ex);
			Assert.Equal("Tests marked with Timeout are only supported for async tests", ex.Message);
			Assert.False(invoker.CancellationTokenSource.IsCancellationRequested);
		}

		[Fact]
		public async ValueTask WithoutTimeout_AsyncMethod_RunsTestWithTimeout()
		{
			var invoker = new TestableXunitTestInvoker(nameof(ClassUnderTest.TimeoutWithAsync), timeout: 10);

			await invoker.RunAsync();

			var ex = invoker.Aggregator.ToException();
			Assert.IsType<TestTimeoutException>(ex);
			Assert.Equal("Test execution timed out after 10 milliseconds", ex.Message);
			Assert.True(invoker.InvokeTestMethodAsync_CancellationToken?.IsCancellationRequested);
		}
	}

	class ClassUnderTest
	{
		public bool WithoutTimeout_Called;

		[Fact]
		public void WithoutTimeout() => WithoutTimeout_Called = true;

		[Fact]
		public void TimeoutWithoutAsync() { }

		[Fact]
		public Task TimeoutWithAsync() => Task.Delay(100);
	}

	class TestableXunitTestInvoker(
		string methodName,
		object?[]? testMethodArguments = null,
		int timeout = 0,
		CancellationTokenSource? cancellationTokenSource = null) :
			XunitTestInvoker
	{
		readonly SpyMessageBus messageBus = new();
		readonly IXunitTest test = TestData.XunitTest<ClassUnderTest>(methodName, timeout: timeout);
		readonly object?[] testMethodArguments = testMethodArguments ?? [];

		public readonly ExceptionAggregator Aggregator = new();
		public readonly CancellationTokenSource CancellationTokenSource = cancellationTokenSource ?? new();
		public readonly ClassUnderTest TestClassInstance = new();

		public CancellationToken? InvokeTestMethodAsync_CancellationToken;

		protected override ValueTask<TimeSpan> InvokeTestMethodAsync(XunitTestInvokerContext ctxt)
		{
			InvokeTestMethodAsync_CancellationToken = TestContext.Current.CancellationToken;

			return base.InvokeTestMethodAsync(ctxt);
		}

		public ValueTask<TimeSpan> RunAsync() =>
			RunAsync(test, TestClassInstance, testMethodArguments, ExplicitOption.Off, messageBus, Aggregator, CancellationTokenSource);
	}
}
