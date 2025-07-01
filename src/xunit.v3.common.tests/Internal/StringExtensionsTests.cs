using Xunit;
using Xunit.Internal;

public class StringExtensionsTests
{
	public class SplitAtOuterCommas
	{
		[Fact]
		public void NoCommas()
		{
			var result = StringExtensions.SplitAtOuterCommas("hello");

			var first = Assert.Single(result);
			Assert.Equal("hello", first);
		}

		[Fact]
		public void CommasOutsideSquareBrackets()
		{
			var result = StringExtensions.SplitAtOuterCommas("hello, world");

			Assert.Collection(
				result,
				part => Assert.Equal("hello", part),
				part => Assert.Equal(" world", part)
			);
		}

		[Fact]
		public void CommasInsideSquareBrackets()
		{
			var result = StringExtensions.SplitAtOuterCommas("hello, [there, my], friend");

			Assert.Collection(
				result,
				part => Assert.Equal("hello", part),
				part => Assert.Equal(" [there, my]", part),
				part => Assert.Equal(" friend", part)
			);
		}
	}
}
