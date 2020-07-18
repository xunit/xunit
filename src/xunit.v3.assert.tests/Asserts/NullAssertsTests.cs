using System;
using Xunit;
using Xunit.Sdk;

public class NullAssertsTests
{
	public class NotNull
	{
		[Fact]
		public void Success()
		{
			Assert.NotNull(new object());
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.NotNull(null));

			Assert.IsType<NotNullException>(ex);
			Assert.Equal("Assert.NotNull() Failure", ex.Message);
		}
	}

	public class Null
	{
		[Fact]
		public void Success()
		{
			Assert.Null(null);
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.Null(new object()));

			Assert.IsType<NullException>(ex);
			Assert.Equal(
				"Assert.Null() Failure" + Environment.NewLine +
				"Expected: (null)" + Environment.NewLine +
				"Actual:   Object { }",
				ex.Message
			);
		}
	}
}
