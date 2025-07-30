using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class CulturedTheoryAttributeTests : AcceptanceTestV3
{
	[Fact]
	public async ValueTask NoCultures()
	{
		var results = await RunForResultsAsync(typeof(TestClassWithNoCultures));

		var result = Assert.Single(results);
		var failed = Assert.IsType<TestFailedWithDisplayName>(result);
		Assert.Equal($"{typeof(TestClassWithNoCultures).FullName}.{nameof(TestClassWithNoCultures.TestMethod)}(_: 42)", failed.TestDisplayName);
		Assert.Equal("Xunit.CulturedTheoryAttribute did not provide any cultures", failed.Messages.Single());
	}

	class TestClassWithNoCultures
	{
		[CulturedTheory([])]
		[InlineData(42)]
		public void TestMethod(int _) { }
	}

	[Fact]
	public async ValueTask SingleCulture()
	{
		var results = await RunForResultsAsync(typeof(TestClassWithSingleCulture));

		var result = Assert.Single(results);
		var passed = Assert.IsType<TestPassedWithDisplayName>(result);
		Assert.Equal($"{typeof(TestClassWithSingleCulture).FullName}.{nameof(TestClassWithSingleCulture.TestMethod)}(_: 42)[fr-FR]", passed.TestDisplayName);
	}

	class TestClassWithSingleCulture
	{
		[CulturedTheory(["fr-FR"])]
		[InlineData(42)]
		public void TestMethod(int _) =>
			Assert.Equal("fr-FR", CultureInfo.CurrentCulture.Name);
	}

	[Fact]
	public async ValueTask MultipleCultures()
	{
		var results = await RunForResultsAsync(typeof(TestClassWithMultipleCultures));

		Assert.Collection(
			results.OfType<TestPassedWithDisplayName>().OrderBy(passed => passed.TestDisplayName),
			passed => Assert.Equal($"{typeof(TestClassWithMultipleCultures).FullName}.{nameof(TestClassWithMultipleCultures.TestMethod)}(_: 2112)[fr-FR]", passed.TestDisplayName),
			passed => Assert.Equal($"{typeof(TestClassWithMultipleCultures).FullName}.{nameof(TestClassWithMultipleCultures.TestMethod)}(_: 42)[fr-FR]", passed.TestDisplayName)
		);
		Assert.Collection(
			results.OfType<TestFailedWithDisplayName>().OrderBy(failed => failed.TestDisplayName),
			failed =>
			{
				Assert.Equal($"{typeof(TestClassWithMultipleCultures).FullName}.{nameof(TestClassWithMultipleCultures.TestMethod)}(_: 2112)[en-US]", failed.TestDisplayName);
				Assert.Equal(typeof(EqualException).FullName, failed.ExceptionTypes.Single());
			},
			failed =>
			{
				Assert.Equal($"{typeof(TestClassWithMultipleCultures).FullName}.{nameof(TestClassWithMultipleCultures.TestMethod)}(_: 42)[en-US]", failed.TestDisplayName);
				Assert.Equal(typeof(EqualException).FullName, failed.ExceptionTypes.Single());
			}
		);
	}

	class TestClassWithMultipleCultures
	{
		[CulturedTheory(["en-US", "fr-FR"])]
		[InlineData(42)]
		[InlineData(2112)]
		public void TestMethod(int _) =>
			Assert.Equal("fr-FR", CultureInfo.CurrentCulture.Name);
	}
}
