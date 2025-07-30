using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class CulturedFactAttributeTests : AcceptanceTestV3
{
	[Fact]
	public async ValueTask NoCultures()
	{
		var results = await RunForResultsAsync(typeof(TestClassWithNoCultures));

		var result = Assert.Single(results);
		var failed = Assert.IsType<TestFailedWithDisplayName>(result);
		Assert.Equal($"{typeof(TestClassWithNoCultures).FullName}.{nameof(TestClassWithNoCultures.TestMethod)}", failed.TestDisplayName);
		Assert.Equal("Xunit.CulturedFactAttribute did not provide any cultures", failed.Messages.Single());
	}

	class TestClassWithNoCultures
	{
		[CulturedFact([])]
		public void TestMethod() { }
	}

	[Fact]
	public async ValueTask SingleCulture()
	{
		var results = await RunForResultsAsync(typeof(TestClassWithSingleCulture));

		var result = Assert.Single(results);
		var passed = Assert.IsType<TestPassedWithDisplayName>(result);
		Assert.Equal($"{typeof(TestClassWithSingleCulture).FullName}.{nameof(TestClassWithSingleCulture.TestMethod)}[fr-FR]", passed.TestDisplayName);
	}

	class TestClassWithSingleCulture
	{
		[CulturedFact(["fr-FR"])]
		public void TestMethod() =>
			Assert.Equal("fr-FR", CultureInfo.CurrentCulture.Name);
	}

	[Fact]
	public async ValueTask MultipleCultures()
	{
		var results = await RunForResultsAsync(typeof(TestClassWithMultipleCultures));

		var passed = Assert.Single(results.OfType<TestPassedWithDisplayName>());
		Assert.Equal($"{typeof(TestClassWithMultipleCultures).FullName}.{nameof(TestClassWithMultipleCultures.TestMethod)}[fr-FR]", passed.TestDisplayName);
		var failed = Assert.Single(results.OfType<TestFailedWithDisplayName>());
		Assert.Equal($"{typeof(TestClassWithMultipleCultures).FullName}.{nameof(TestClassWithMultipleCultures.TestMethod)}[en-US]", failed.TestDisplayName);
		Assert.Equal(typeof(EqualException).FullName, failed.ExceptionTypes.Single());
	}

	class TestClassWithMultipleCultures
	{
		[CulturedFact(["en-US", "fr-FR"])]
		public void TestMethod() =>
			Assert.Equal("fr-FR", CultureInfo.CurrentCulture.Name);
	}
}
