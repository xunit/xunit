#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

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
		public unsafe void Success_Pointer()
		{
			var x = 42;
			Assert.NotNull(&x);

			var y = "Hello world";
			Assert.NotNull(&y);
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

		[Fact]
		public unsafe void Failure_Pointer()
		{
			var ex = Record.Exception(() => Assert.NotNull((object*)null));

			Assert.IsType<NotNullException>(ex);
			Assert.Equal("Assert.NotNull() Failure: Value of type 'object*' is null", ex.Message);
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
		public unsafe void Success_Pointer()
		{
			Assert.Null((object*)null);
		}

		[Fact]
		public void Failure_Reference()
		{
			var ex = Record.Exception(() => Assert.Null(new object()));

			Assert.IsType<NullException>(ex);
			Assert.Equal(
				"Assert.Null() Failure: Value is not null" + Environment.NewLine +
				"Expected: null" + Environment.NewLine +
#if XUNIT_AOT
				$"Actual:   Object {{ {ArgumentFormatter.Ellipsis} }}",
#else
				"Actual:   Object { }",
#endif
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

		[Fact]
		public void Failure_Pointer()
		{
			verifyFailure(42);
			verifyFailure("Hello world");

			static unsafe void verifyFailure<T>(T data)
			{
				var ptr = &data;

				var ex = Record.Exception(() => Assert.Null(ptr));

				Assert.IsType<NullException>(ex);
				Assert.Equal($"Assert.Null() Failure: Value of type '{ArgumentFormatter.FormatTypeName(typeof(T))}*' is not null", ex.Message);
			}
		}
	}
}
