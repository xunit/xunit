using System;
using Xunit;
using Xunit.Sdk;

public class TypeAssertsTests
{
	public class IsAssignableFrom_Generic
	{
		[Fact]
		public void NullObjectThrows()
		{
			var ex = Record.Exception(() => Assert.IsAssignableFrom<object>(null));

			Assert.IsType<IsAssignableFromException>(ex);
			Assert.Equal(
				"Assert.IsAssignableFrom() Failure: Value is null" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				"Actual:   null",
				ex.Message
			);
		}

		[Fact]
		public void SameType()
		{
			var expected = new InvalidCastException();

			Assert.IsAssignableFrom<InvalidCastException>(expected);
		}

		[Fact]
		public void BaseType()
		{
			var expected = new InvalidCastException();

			Assert.IsAssignableFrom<Exception>(expected);
		}

		[Fact]
		public void Interface()
		{
			var expected = new DisposableClass();

			Assert.IsAssignableFrom<IDisposable>(expected);
		}

		[Fact]
		public void ReturnsCastObject()
		{
			var expected = new InvalidCastException();

			var actual = Assert.IsAssignableFrom<InvalidCastException>(expected);

			Assert.Same(expected, actual);
		}

		[Fact]
		public void IncompatibleTypeThrows()
		{
			var ex =
				Record.Exception(
					() => Assert.IsAssignableFrom<InvalidCastException>(new InvalidOperationException())
				);

			Assert.IsType<IsAssignableFromException>(ex);
			Assert.Equal(
				"Assert.IsAssignableFrom() Failure: Value is an incompatible type" + Environment.NewLine +
				"Expected: typeof(System.InvalidCastException)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidOperationException)",
				ex.Message
			);
		}
	}

	public class IsAssignableFrom_NonGeneric
	{
		[Fact]
		public void NullObjectThrows()
		{
			var ex = Record.Exception(() => Assert.IsAssignableFrom(typeof(object), null));

			Assert.IsType<IsAssignableFromException>(ex);
			Assert.Equal(
				"Assert.IsAssignableFrom() Failure: Value is null" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				"Actual:   null",
				ex.Message
			);
		}

		[Fact]
		public void SameType()
		{
			var expected = new InvalidCastException();

			Assert.IsAssignableFrom(typeof(InvalidCastException), expected);
		}

		[Fact]
		public void BaseType()
		{
			var expected = new InvalidCastException();

			Assert.IsAssignableFrom(typeof(Exception), expected);
		}

		[Fact]
		public void Interface()
		{
			var expected = new DisposableClass();

			Assert.IsAssignableFrom(typeof(IDisposable), expected);
		}

		[Fact]
		public void ReturnsCastObject()
		{
			var expected = new InvalidCastException();

			var actual = Assert.IsAssignableFrom<InvalidCastException>(expected);

			Assert.Same(expected, actual);
		}

		[Fact]
		public void IncompatibleTypeThrows()
		{
			var ex =
				Record.Exception(
					() => Assert.IsAssignableFrom(typeof(InvalidCastException), new InvalidOperationException())
				);

			Assert.IsType<IsAssignableFromException>(ex);
			Assert.Equal(
				"Assert.IsAssignableFrom() Failure: Value is an incompatible type" + Environment.NewLine +
				"Expected: typeof(System.InvalidCastException)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidOperationException)",
				ex.Message
			);
		}
	}

	public class IsNotType_Generic
	{
		[Fact]
		public void UnmatchedType()
		{
			var expected = new InvalidCastException();

			Assert.IsNotType<Exception>(expected);
		}

		[Fact]
		public void MatchedTypeThrows()
		{
			var ex = Record.Exception(() => Assert.IsNotType<InvalidCastException>(new InvalidCastException()));

			Assert.IsType<IsNotTypeException>(ex);
			Assert.Equal(
				"Assert.IsNotType() Failure: Value is the exact type" + Environment.NewLine +
				"Expected: typeof(System.InvalidCastException)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidCastException)",
				ex.Message
			);
		}

		[Fact]
		public void NullObjectDoesNotThrow()
		{
			Assert.IsNotType<object>(null);
		}
	}

	public class IsNotType_NonGeneric
	{
		[Fact]
		public void UnmatchedType()
		{
			var expected = new InvalidCastException();

			Assert.IsNotType(typeof(Exception), expected);
		}

		[Fact]
		public void MatchedTypeThrows()
		{
			var ex = Record.Exception(() => Assert.IsNotType(typeof(InvalidCastException), new InvalidCastException()));

			Assert.IsType<IsNotTypeException>(ex);
			Assert.Equal(
				"Assert.IsNotType() Failure: Value is the exact type" + Environment.NewLine +
				"Expected: typeof(System.InvalidCastException)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidCastException)",
				ex.Message
			);
		}

		[Fact]
		public void NullObjectDoesNotThrow()
		{
			Assert.IsNotType(typeof(object), null);
		}
	}

	public class IsType_Generic
	{
		[Fact]
		public void MatchingType()
		{
			var expected = new InvalidCastException();

			Assert.IsType<InvalidCastException>(expected);
		}

		[Fact]
		public void ReturnsCastObject()
		{
			var expected = new InvalidCastException();

			var actual = Assert.IsType<InvalidCastException>(expected);

			Assert.Same(expected, actual);
		}

		[Fact]
		public void UnmatchedTypeThrows()
		{
			XunitException exception =
				Assert.Throws<IsTypeException>(
					() => Assert.IsType<InvalidCastException>(new InvalidOperationException()));

			Assert.Equal("Assert.IsType() Failure", exception.UserMessage);
		}

		[Fact]
		public void NullObjectThrows()
		{
			Assert.Throws<IsTypeException>(() => Assert.IsType<object>(null));
		}
	}

	public class IsType_NonGeneric
	{
		[Fact]
		public void MatchingType()
		{
			var expected = new InvalidCastException();

			Assert.IsType(typeof(InvalidCastException), expected);
		}

		[Fact]
		public void UnmatchedTypeThrows()
		{
			XunitException exception =
				Assert.Throws<IsTypeException>(
					() => Assert.IsType(typeof(InvalidCastException), new InvalidOperationException()));

			Assert.Equal("Assert.IsType() Failure", exception.UserMessage);
		}

		[Fact]
		public void NullObjectThrows()
		{
			Assert.Throws<IsTypeException>(() => Assert.IsType(typeof(object), null));
		}
	}

	class DisposableClass : IDisposable
	{
		public void Dispose()
		{ }
	}
}
