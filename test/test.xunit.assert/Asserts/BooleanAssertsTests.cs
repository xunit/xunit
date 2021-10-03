using System;
using Xunit;
using Xunit.Sdk;

public class BooleanAssertsTests
{
	public class False
	{
		[Fact]
		public static void AssertFalse()
		{
			Assert.False(false);
		}

		[Fact]
		public static void ThrowsExceptionWhenTrue()
		{
			var ex = Record.Exception(() => Assert.False(true));

			Assert.IsType<FalseException>(ex);
			Assert.Equal(
				"Assert.False() Failure" + Environment.NewLine +
				"Expected: False" + Environment.NewLine +
				"Actual:   True",
				ex.Message
			);
		}

		[Fact]
		public static void ThrowsExceptionWhenNull()
		{
			var ex = Record.Exception(() => Assert.False(null));

			Assert.IsType<FalseException>(ex);
			Assert.Equal(
				"Assert.False() Failure" + Environment.NewLine +
				"Expected: False" + Environment.NewLine +
				"Actual:   (null)",
				ex.Message
			);
		}

		[Fact]
		public static void UserSuppliedMessage()
		{
			var ex = Record.Exception(() => Assert.False(true, "Custom User Message"));

			Assert.NotNull(ex);
			Assert.Equal(
				"Custom User Message" + Environment.NewLine +
				"Expected: False" + Environment.NewLine +
				"Actual:   True",
				ex.Message
			);
		}
	}

	public class True
	{
		[Fact]
		public static void AssertTrue()
		{
			Assert.True(true);
		}

		[Fact]
		public static void ThrowsExceptionWhenFalse()
		{
			var ex = Record.Exception(() => Assert.True(false));

			Assert.IsType<TrueException>(ex);
			Assert.Equal(
				"Assert.True() Failure" + Environment.NewLine +
				"Expected: True" + Environment.NewLine +
				"Actual:   False",
				ex.Message
			);
		}

		[Fact]
		public static void ThrowsExceptionWhenNull()
		{
			var ex = Record.Exception(() => Assert.True(null));

			Assert.IsType<TrueException>(ex);
			Assert.Equal(
				"Assert.True() Failure" + Environment.NewLine +
				"Expected: True" + Environment.NewLine +
				"Actual:   (null)",
				ex.Message
			);
		}

		[Fact]
		public static void UserSuppliedMessage()
		{
			var ex = Record.Exception(() => Assert.True(false, "Custom User Message"));

			Assert.NotNull(ex);
			Assert.Equal(
				"Custom User Message" + Environment.NewLine +
				"Expected: True" + Environment.NewLine +
				"Actual:   False",
				ex.Message
			);
		}
	}
}
