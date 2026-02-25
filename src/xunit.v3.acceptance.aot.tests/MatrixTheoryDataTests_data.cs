using Xunit;

public partial class MatrixTheoryDataTests
{
#if XUNIT_AOT
	public
#endif
	class SampleUsage
	{
		public static int[] Numbers = [42, 5, 6];
		public static string[] Strings = ["Hello", "world!"];
		public static MatrixTheoryData<string, int> MatrixData = new(Strings, Numbers);

		[Theory]
		[MemberData(nameof(MatrixData))]
		public void MyTestMethod(string x, int y)
		{
			Assert.Equal(y, x.Length);
		}
	}
}
