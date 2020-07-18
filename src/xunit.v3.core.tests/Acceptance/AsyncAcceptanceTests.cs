using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

public class AsyncAcceptanceTests : AcceptanceTestV3
{
	[Fact]
	public async void AsyncTaskTestsRunCorrectly()
	{
		var results = await RunAsync<ITestResultMessage>(typeof(ClassWithAsyncTask));

		var result = Assert.Single(results);
		var failed = Assert.IsAssignableFrom<ITestFailed>(result);
		Assert.Equal("Xunit.Sdk.EqualException", failed.ExceptionTypes.Single());
	}

	[Fact]
	public async void AsyncVoidTestsRunCorrectly()
	{
		var results = await RunAsync<ITestResultMessage>(typeof(ClassWithAsyncVoid));

		var result = Assert.Single(results);
		var failed = Assert.IsAssignableFrom<ITestFailed>(result);
		Assert.Equal("Xunit.Sdk.EqualException", failed.ExceptionTypes.Single());
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

	class ClassWithAsyncVoid
	{
		[Fact]
		public async void AsyncTest_Void()
		{
			var result = await Task.FromResult(21);

			Assert.Equal(42, result);
		}
	}
}
