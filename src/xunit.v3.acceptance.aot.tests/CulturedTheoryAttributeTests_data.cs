using Xunit;

public partial class CulturedTheoryAttributeTests
{
#if XUNIT_AOT
	public
#endif
	static class TestClassWithSingleCulture
	{
		[CulturedTheory(["fr-FR"])]
		[InlineData(42)]
		public static void TestMethod(int _) =>
			Assert.Equal("fr-FR", CultureInfo.CurrentCulture.Name);
	}

#if XUNIT_AOT
	public
#endif
	class TestClassWithMultipleCultures
	{
		[CulturedTheory(["en-US", "fr-FR"])]
		[InlineData(42)]
		[InlineData(2112)]
		public static void TestMethod(int _) =>
			Assert.Equal("fr-FR", CultureInfo.CurrentCulture.Name);
	}
}
