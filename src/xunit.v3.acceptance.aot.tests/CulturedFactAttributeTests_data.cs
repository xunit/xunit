using Xunit;

public partial class CulturedFactAttributeTests
{
#if XUNIT_AOT
	public
#endif
	class TestClassWithSingleCulture
	{
		[CulturedFact(["fr-FR"])]
		public void TestMethod() =>
			Assert.Equal("fr-FR", CultureInfo.CurrentCulture.Name);
	}

#if XUNIT_AOT
	public
#endif
	class TestClassWithMultipleCultures
	{
		[CulturedFact(["en-US", "fr-FR"])]
		public void TestMethod() =>
			Assert.Equal("fr-FR", CultureInfo.CurrentCulture.Name);
	}
}
