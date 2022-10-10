namespace Issue_2585
{
	public class Test
	{
		[Theory]
		[InlineData("cherry")]
		[InlineData("banana")]
		//[InlineData("Apple")]
		public virtual void TestMethod(string a)
		{
			Assert.True(true);

		}
	}

	public class MyTest : Test
	{
		[Theory]
		[InlineData("orange")]
		[InlineData("pineapple")]
		public override void TestMethod(string a)
		{
			Assert.True(true);
		}
	}
}
