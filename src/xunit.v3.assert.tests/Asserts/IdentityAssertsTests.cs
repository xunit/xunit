using System;
using Xunit;
using Xunit.Sdk;

public class IdentityAssertsTests
{
	public class NotSame
	{
		[Fact]
		public void Identical()
		{
			var actual = new object();

			var ex = Record.Exception(() => Assert.NotSame(actual, actual));

			Assert.IsType<NotSameException>(ex);
			Assert.Equal("Assert.NotSame() Failure: Values are the same instance", ex.Message);
		}

		[Fact]
		public void NotIdentical()
		{
			Assert.NotSame("bob", "jim");
		}
	}

	public class Same
	{
		[Fact]
		public void Identical()
		{
			var actual = new object();

			Assert.Same(actual, actual);
		}

		[Fact]
		public void NotIdentical()
		{
			var ex = Record.Exception(() => Assert.Same("bob", "jim"));

			Assert.IsType<SameException>(ex);
			Assert.Equal(
				"Assert.Same() Failure: Values are not the same instance" + Environment.NewLine +
				"Expected: \"bob\"" + Environment.NewLine +
				"Actual:   \"jim\"",
				ex.Message
			);
		}

		[Fact]
		public void EqualValueTypeValuesAreNotSameBecauseOfBoxing()
		{
#pragma warning disable xUnit2005 // Do not use identity check on value type
			Assert.Throws<SameException>(() => Assert.Same(0, 0));
#pragma warning restore xUnit2005 // Do not use identity check on value type
		}
	}
}
