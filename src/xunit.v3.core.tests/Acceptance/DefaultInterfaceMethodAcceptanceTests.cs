#if NETCOREAPP

using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class DefaultInterfaceMethodAcceptanceTests : AcceptanceTestV3
{
	[Fact]
	public async ValueTask AcceptanceTest()
	{
		var results = await RunForResultsAsync(typeof(ClassUnderTest));

		var passed = Assert.Single(results.OfType<TestPassedWithDisplayName>());
		Assert.Equal($"{typeof(ClassUnderTest).FullName}.{nameof(IHaveDefaultMethods.Passing)}", passed.TestDisplayName);

		var failed = Assert.Single(results.OfType<TestFailedWithDisplayName>());
		Assert.Equal($"{typeof(ClassUnderTest).FullName}.{nameof(IHaveDefaultMethods.Failing)}", failed.TestDisplayName);
		Assert.Equal(typeof(TrueException).FullName, failed.ExceptionTypes.Single());

		var skipped = Assert.Single(results.OfType<TestSkippedWithDisplayName>());
		Assert.Equal($"{typeof(ClassUnderTest).FullName}.{nameof(IHaveDefaultMethods.Skipping)}", skipped.TestDisplayName);
		Assert.Equal("Statically skipped", skipped.Reason);
	}

	interface IHaveDefaultMethods
	{
		[Fact]
		public void Passing() => Assert.IsType<ClassUnderTest>(this);

		[Fact]
		public void Failing() => Assert.True(false);

		[Fact(Skip = "Statically skipped")]
		public void Skipping() => Assert.True(false);

		void NotATest();
	}

	class ClassUnderTest : IHaveDefaultMethods
	{
		public void NotATest() { }
	}
}

#endif
