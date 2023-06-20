using System;
using Xunit;
using Xunit.Sdk;

public class NullAssertsTests
{
	public class NotNull
	{
		[Fact]
		public void Success_Reference()
		{
			Assert.NotNull(new object());
		}

		[Fact]
		public void Success_NullableStruct()
		{
			int? x = 42;

			var result = Assert.NotNull(x);

			Assert.IsType<int>(result);
			Assert.Equal(42, result);
		}

		[Fact]
		public void Failure_Reference()
		{
			var ex = Record.Exception(() => Assert.NotNull(null));

			Assert.IsType<NotNullException>(ex);
			Assert.Equal("Assert.NotNull() Failure: Value is null", ex.Message);
		}

		[Fact]
		public void Failure_NullableStruct()
		{
			int? value = null;

			var ex = Record.Exception(() => Assert.NotNull(value));

			Assert.IsType<NotNullException>(ex);
			Assert.Equal("Assert.NotNull() Failure: Value of type 'Nullable<int>' does not have a value", ex.Message);
		}
	}

	public class Null
	{
		[Fact]
		public void Success_Reference()
		{
			Assert.Null(null);
		}

		[Fact]
		public void Success_NullableStruct()
		{
			int? x = null;

			Assert.Null(x);
		}

		[Fact]
		public void Failure_Reference()
		{
			var ex = Record.Exception(() => Assert.Null(new object()));

			Assert.IsType<NullException>(ex);
			Assert.Equal(
				"Assert.Null() Failure: Value is not null" + Environment.NewLine +
				"Expected: null" + Environment.NewLine +
				"Actual:   Object { }",
				ex.Message
			);
		}

		[Fact]
		public void Failure_NullableStruct()
		{
			int? x = 42;

			var ex = Record.Exception(() => Assert.Null(x));

			Assert.IsType<NullException>(ex);
			Assert.Equal(
				"Assert.Null() Failure: Value of type 'Nullable<int>' has a value" + Environment.NewLine +
				"Expected: null" + Environment.NewLine +
				"Actual:   42",
				ex.Message
			);
		}
	}
}
