using System;
using System.Linq;
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
		var results = await RunAsync<TestFailed>(classUnderTest);

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
}
