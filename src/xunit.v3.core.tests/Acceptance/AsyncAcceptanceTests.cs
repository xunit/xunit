using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class AsyncAcceptanceTests : AcceptanceTestV3
{
	[Theory]
	[InlineData(typeof(ClassWithAsyncValueTask))]
	[InlineData(typeof(ClassWithAsyncTask))]
	[InlineData(typeof(ClassWithAsyncVoid))]
	public async ValueTask AsyncValueTaskTestsRunCorrectly(Type classUnderTest)
	{
		var results = await RunAsync<_TestFailed>(classUnderTest);

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
