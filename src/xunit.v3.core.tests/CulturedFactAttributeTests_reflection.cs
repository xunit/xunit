using Xunit;

partial class CulturedFactAttributeTests
{
	// Native AOT reports these in the generator
	[Fact]
	public static async ValueTask NoCultures()
	{
		var results = await RunForResultsAsync(typeof(TestClassWithNoCultures));

		var result = Assert.Single(results);
		var failed = Assert.IsType<TestFailedWithMetadata>(result);
		Assert.Equal($"{typeof(TestClassWithNoCultures).FullName}.{nameof(TestClassWithNoCultures.TestMethod)}", failed.Test.TestDisplayName);
		Assert.Equal("Xunit.CulturedFactAttribute did not provide any cultures", failed.Messages.Single());
	}

	class TestClassWithNoCultures
	{
#pragma warning disable xUnit1060 // Cultured test methods must have at least one culture
		[CulturedFact([])]
#pragma warning restore xUnit1060 // Cultured test methods must have at least one culture
		public void TestMethod() { }
	}
}
