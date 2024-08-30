using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

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
}
