using Xunit;
using Xunit.Sdk;

public partial class CulturedFactAttributeTests : AcceptanceTestV3
{
	[Fact]
	public async ValueTask SingleCulture()
	{
#if XUNIT_AOT
		var results = await RunForResultsAsync("CulturedFactAttributeTests+TestClassWithSingleCulture");
#else
		var results = await RunForResultsAsync(typeof(TestClassWithSingleCulture));
#endif

		var result = Assert.Single(results);
		var passed = Assert.IsType<TestPassedWithMetadata>(result);
		Assert.Equal("CulturedFactAttributeTests+TestClassWithSingleCulture.TestMethod[fr-FR]", passed.Test.TestDisplayName);
	}

	[Fact]
	public async ValueTask MultipleCultures()
	{
#if XUNIT_AOT
		var results = await RunForResultsAsync("CulturedFactAttributeTests+TestClassWithMultipleCultures");
#else
		var results = await RunForResultsAsync(typeof(TestClassWithMultipleCultures));
#endif

		var passed = Assert.Single(results.OfType<TestPassedWithMetadata>());
		Assert.Equal("CulturedFactAttributeTests+TestClassWithMultipleCultures.TestMethod[fr-FR]", passed.Test.TestDisplayName);
		var failed = Assert.Single(results.OfType<TestFailedWithMetadata>());
		Assert.Equal("CulturedFactAttributeTests+TestClassWithMultipleCultures.TestMethod[en-US]", failed.Test.TestDisplayName);
		Assert.Equal(typeof(EqualException).FullName, failed.ExceptionTypes.Single());
	}
}
