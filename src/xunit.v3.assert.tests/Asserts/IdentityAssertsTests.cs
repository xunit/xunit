using Xunit;
using Xunit.Sdk;

public class IdentityAssertsTests
{
	public class NotSame
	{
		[Fact]
		public void Success()
		{
			Assert.NotSame("bob", "jim");
		}

		[Fact]
		public void Failure()
		{
			var actual = new object();

			var ex = Record.Exception(() => Assert.NotSame(actual, actual));

			Assert.IsType<NotSameException>(ex);
			Assert.Equal("Assert.NotSame() Failure", ex.Message);
		}
	}

	public class Same
	{
		[Fact]
		public void Success()
		{
			Assert.Throws<SameException>(() => Assert.Same("bob", "jim"));
		}

		[Fact]
		public void Failure()
		{
			var actual = "Abc";
			var expected = "a".ToUpperInvariant() + "bc";

			var ex = Record.Exception(() => Assert.Same(expected, actual));

			var sex = Assert.IsType<SameException>(ex);
			Assert.Equal("Assert.Same() Failure", sex.UserMessage);
			Assert.DoesNotContain("Position:", sex.Message);
		}

		[Fact]
		public void BoxedTypesDontWork()
		{
			var index = 0;

			Assert.Throws<SameException>(() => Assert.Same(index, index));
		}
	}
}
