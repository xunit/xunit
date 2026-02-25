#if NETCOREAPP

using Xunit;
using Xunit.Sdk;

// Default interface methods are not supported on .NET Framework or Native AOT
public class DefaultInterfaceMethodAcceptanceTests : AcceptanceTestV3
{
	[Fact]
	public async ValueTask AcceptanceTest()
	{
		var results = await RunForResultsAsync(typeof(ClassUnderTest));

		var passed = Assert.Single(results.OfType<TestPassedWithMetadata>());
		Assert.Equal($"{typeof(ClassUnderTest).FullName}.{nameof(IHaveDefaultMethods.Passing)}", passed.Test.TestDisplayName);

		var failed = Assert.Single(results.OfType<TestFailedWithMetadata>());
		Assert.Equal($"{typeof(ClassUnderTest).FullName}.{nameof(IHaveDefaultMethods.Failing)}", failed.Test.TestDisplayName);
		Assert.Equal(typeof(TrueException).FullName, failed.ExceptionTypes.Single());

		var skipped = Assert.Single(results.OfType<TestSkippedWithMetadata>());
		Assert.Equal($"{typeof(ClassUnderTest).FullName}.{nameof(IHaveDefaultMethods.Skipping)}", skipped.Test.TestDisplayName);
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
