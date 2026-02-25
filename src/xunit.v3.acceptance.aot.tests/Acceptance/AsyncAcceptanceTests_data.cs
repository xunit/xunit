using Xunit;

public partial class AsyncAcceptanceTests
{
	public partial class Tasks
	{
#if XUNIT_AOT
		public
#endif
		class ClassWithAsyncValueTask
		{
			[Fact]
			public async ValueTask AsyncTest()
			{
				var result = await Task.FromResult(21);

				Assert.Equal(42, result);
			}
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithAsyncTask
		{
			[Fact]
			public async Task AsyncTest()
			{
				var result = await Task.FromResult(21);

				Assert.Equal(42, result);
			}
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithTaskCancelledException
		{
			[Fact]
			public async Task TestMethod()
			{
				await Task.Yield();
				throw new TaskCanceledException("manually throwing");
			}
		}
	}
}
