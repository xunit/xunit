using System;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class AsyncAcceptanceTests : AcceptanceTestV3
{
	[Theory]
	[InlineData(typeof(ClassWithAsyncValueTask))]
	[InlineData(typeof(ClassWithAsyncTask))]
	public async ValueTask AsyncTestsRunCorrectly(Type classUnderTest)
	{
		var results = await RunAsync<ITestFailed>(classUnderTest);

		var failed = Assert.Single(results);
		Assert.Equal(typeof(EqualException).FullName, failed.ExceptionTypes.Single());
	}

	class ClassWithAsyncValueTask
	{
		[Fact]
		public async ValueTask AsyncTest()
		{
			var result = await Task.FromResult(21);

			Assert.Equal(42, result);
		}
	}

	class ClassWithAsyncTask
	{
		[Fact]
		public async Task AsyncTest()
		{
			var result = await Task.FromResult(21);

			Assert.Equal(42, result);
		}
	}

	// https://github.com/xunit/xunit/issues/3153
	[Fact]
	public async ValueTask AsyncMethodWhichThrowsTaskCancelledException()
	{
		var results = await RunForResultsAsync(typeof(ClassWithTaskCancelledException));

		var failed = Assert.Single(results.OfType<TestFailedWithDisplayName>());
		Assert.Equal($"{typeof(ClassWithTaskCancelledException).FullName}.{nameof(ClassWithTaskCancelledException.TestMethod)}", failed.TestDisplayName);
		var exception = Assert.Single(failed.ExceptionTypes);
		Assert.Equal(typeof(TaskCanceledException).FullName, exception);
	}

	class ClassWithTaskCancelledException
	{
		[Fact]
		public async Task TestMethod()
		{
			await Task.Yield();
			throw new TaskCanceledException("manually throwing");
		}
	}

	public sealed class CustomSynchronizationContext : IDisposable
	{
		public CustomSynchronizationContext() =>
			SynchronizationContext.SetSynchronizationContext(new CustomSyncContext(SynchronizationContext.Current));

		public void Dispose() =>
			Assert.IsType<CustomSyncContext>(SynchronizationContext.Current);

		// https://github.com/xunit/xunit/issues/3014
		[Fact]
		public void SyncContextSetInConstructorPropagates() =>
			Assert.IsType<CustomSyncContext>(SynchronizationContext.Current);

		class CustomSyncContext(SynchronizationContext? innerContext) :
			SynchronizationContext
		{
			public override void OperationCompleted() => innerContext?.OperationCompleted();
			public override void OperationStarted() => innerContext?.OperationStarted();
			public override void Post(SendOrPostCallback d, object? state) => innerContext?.Post(d, state);
			public override void Send(SendOrPostCallback d, object? state) => innerContext?.Send(d, state);
		}
	}

	public class AsyncLocalUsage
	{
		static readonly AsyncLocal<int> asyncLocal = new();

		public AsyncLocalUsage() =>
			asyncLocal.Value = 42;

		// https://github.com/xunit/xunit/issues/3033
		[Fact]
		public void Test() =>
			Assert.Equal(42, asyncLocal.Value);
	}

	[PrincipalBeforeAfter]
	public class PrincipalUsage
	{
		[Fact]
		public void Test() =>
			Assert.Equal("xUnit", Thread.CurrentPrincipal?.Identity?.Name);
	}

	public class PrincipalBeforeAfterAttribute : BeforeAfterTestAttribute
	{
		IPrincipal? originalPrincipal;

		public override void After(MethodInfo methodUnderTest, IXunitTest test)
		{
			if (originalPrincipal is not null)
				Thread.CurrentPrincipal = originalPrincipal;
		}

		public override void Before(MethodInfo methodUnderTest, IXunitTest test)
		{
			originalPrincipal = Thread.CurrentPrincipal;

			var identity = new GenericIdentity("xUnit");
			var principal = new GenericPrincipal(identity, ["role1"]);
			Thread.CurrentPrincipal = principal;
		}
	}
}
