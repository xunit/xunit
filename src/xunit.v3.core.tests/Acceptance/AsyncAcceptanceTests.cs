using System.Security.Principal;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

#if !XUNIT_AOT
using System.Reflection;
#endif

public partial class AsyncAcceptanceTests
{
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

	[PrincipalBeforeAfter]
	public class PrincipalUsage
	{
		[Fact]
		public void Test() =>
			Assert.Equal("xUnit", Thread.CurrentPrincipal?.Identity?.Name);

		public class PrincipalBeforeAfterAttribute : BeforeAfterTestAttribute
		{
			IPrincipal? originalPrincipal;

#if XUNIT_AOT
			public override void After(ICodeGenTest test)
#else
			public override void After(
				MethodInfo methodUnderTest,
				IXunitTest test)
#endif
			{
				if (originalPrincipal is not null)
					Thread.CurrentPrincipal = originalPrincipal;
			}

#if XUNIT_AOT
			public override void Before(ICodeGenTest test)
#else
			public override void Before(
				MethodInfo methodUnderTest,
				IXunitTest test)
#endif
			{
				originalPrincipal = Thread.CurrentPrincipal;

				var identity = new GenericIdentity("xUnit");
				var principal = new GenericPrincipal(identity, ["role1"]);
				Thread.CurrentPrincipal = principal;
			}
		}
	}

	public partial class Tasks : AcceptanceTestV3
	{
		[Theory]
#if XUNIT_AOT
		[InlineData("AsyncAcceptanceTests+Tasks+ClassWithAsyncValueTask")]
		[InlineData("AsyncAcceptanceTests+Tasks+ClassWithAsyncTask")]
		public async ValueTask AsyncTestsRunCorrectly(string classUnderTest)
#else
		[InlineData(typeof(ClassWithAsyncValueTask))]
		[InlineData(typeof(ClassWithAsyncTask))]
		public async ValueTask AsyncTestsRunCorrectly(Type classUnderTest)
#endif
		{
			var results = await RunAsync<ITestFailed>(classUnderTest);

			var failed = Assert.Single(results);
			Assert.Equal(typeof(EqualException).FullName, failed.ExceptionTypes.Single());
		}

		// https://github.com/xunit/xunit/issues/3153
		[Fact]
		public async ValueTask AsyncMethodWhichThrowsTaskCancelledException()
		{
#if XUNIT_AOT
			var results = await RunForResultsAsync("AsyncAcceptanceTests+Tasks+ClassWithTaskCancelledException");
#else
			var results = await RunForResultsAsync(typeof(ClassWithTaskCancelledException));
#endif

			var failed = Assert.Single(results.OfType<TestFailedWithMetadata>());
			Assert.Equal("AsyncAcceptanceTests+Tasks+ClassWithTaskCancelledException.TestMethod", failed.Test.TestDisplayName);
			var exception = Assert.Single(failed.ExceptionTypes);
			Assert.Equal(typeof(TaskCanceledException).FullName, exception);
		}
	}
}
