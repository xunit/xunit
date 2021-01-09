using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class EventAssertsTests
{
	public class Raises_Generic
	{
		[Fact]
		public static void ExpectedEventButCodeDoesNotRaise()
		{
			var x = new RaisingClass();

			var recorded = Record.Exception(() => Assert.Raises<object>(h => x.Completed += h, h => x.Completed -= h, () => { }));

			var exception = Assert.IsType<RaisesException>(recorded);
			Assert.Equal("(No event was raised)", exception.Actual);
			Assert.Equal("Object", exception.Expected);
		}

		[Fact]
		public static void ExpectedEventButRaisesEventWithDerivedObject()
		{
			var x = new RaisingClass();

			var recorded = Record.Exception(() =>
				Assert.Raises<object>(
					h => x.Completed += h,
					h => x.Completed -= h,
					() => x.RaiseWithArgs(new DerivedObject())
				)
			);

			var exception = Assert.IsType<RaisesException>(recorded);
			Assert.Equal("DerivedObject", exception.Actual);
			Assert.Equal("Object", exception.Expected);
			Assert.Equal("(Raised event did not match expected event)", exception.UserMessage);
		}

		[Fact]
		public static void GotExpectedEvent()
		{
			var x = new RaisingClass();
			var genericObject = new object();

			var evt = Assert.Raises<object>(
				h => x.Completed += h,
				h => x.Completed -= h,
				() => x.RaiseWithArgs(genericObject)
			);

			Assert.NotNull(evt);
			Assert.Equal(x, evt.Sender);
			Assert.Equal(genericObject, evt.Arguments);
		}
	}

	public class RaisesAny_Generic
	{
		[Fact]
		public static void ExpectedEventButCodeDoesNotRaise()
		{
			var x = new RaisingClass();

			var recorded = Record.Exception(() => Assert.RaisesAny<object>(h => x.Completed += h, h => x.Completed -= h, () => { }));

			var exception = Assert.IsType<RaisesException>(recorded);
			Assert.Equal("(No event was raised)", exception.Actual);
			Assert.Equal("Object", exception.Expected);
		}

		[Fact]
		public static void GotExpectedEvent()
		{
			var x = new RaisingClass();
			var genericObject = new object();

			var evt = Assert.RaisesAny<object>(
				h => x.Completed += h,
				h => x.Completed -= h,
				() => x.RaiseWithArgs(genericObject)
			);

			Assert.NotNull(evt);
			Assert.Equal(x, evt.Sender);
			Assert.Equal(genericObject, evt.Arguments);
		}


		[Fact]
		public static void GotDerivedEvent()
		{
			var x = new RaisingClass();
			var args = new DerivedObject();

			var evt = Assert.RaisesAny<object>(
				h => x.Completed += h,
				h => x.Completed -= h,
				() => x.RaiseWithArgs(args)
			);

			Assert.NotNull(evt);
			Assert.Equal(x, evt.Sender);
			Assert.Equal(args, evt.Arguments);
		}
	}

	public class RaisesAnyAsync_Generic
	{
		[Fact]
		public static async Task ExpectedEventButCodeDoesNotRaise()
		{
			var x = new RaisingClass();

			var recorded = await Record.ExceptionAsync(
				() => Assert.RaisesAnyAsync<object>(
					h => x.Completed += h, h => x.Completed -= h, () => Task.Run(() => { })
				)
			);

			var exception = Assert.IsType<RaisesException>(recorded);
			Assert.Equal("(No event was raised)", exception.Actual);
			Assert.Equal("Object", exception.Expected);
		}

		[Fact]
		public static async Task GotExpectedEvent()
		{
			var x = new RaisingClass();
			var genericObject = new object();

			var evt = await Assert.RaisesAnyAsync<object>(
				h => x.Completed += h,
				h => x.Completed -= h,
				() => Task.Run(() => x.RaiseWithArgs(genericObject))
			);

			Assert.NotNull(evt);
			Assert.Equal(x, evt.Sender);
			Assert.Equal(genericObject, evt.Arguments);
		}

		[Fact]
		public static async Task GotDerivedEvent()
		{
			var x = new RaisingClass();
			var args = new DerivedObject();

			var evt = await Assert.RaisesAnyAsync<object>(
				h => x.Completed += h,
				h => x.Completed -= h,
				() => Task.Run(() => x.RaiseWithArgs(args))
			);

			Assert.NotNull(evt);
			Assert.Equal(x, evt.Sender);
			Assert.Equal(args, evt.Arguments);
		}
	}

	public class RaisesAsync_Generic
	{
		[Fact]
		public static async Task ExpectedEventButCodeDoesNotRaise()
		{
			var x = new RaisingClass();

			var recorded = await Record.ExceptionAsync(
				() => Assert.RaisesAsync<object>(
					h => x.Completed += h, h => x.Completed -= h, () => Task.Run(() => { })
				)
			);

			var exception = Assert.IsType<RaisesException>(recorded);
			Assert.Equal("(No event was raised)", exception.Actual);
			Assert.Equal("Object", exception.Expected);
		}

		[Fact]
		public static async Task ExpectedEventButRaisesEventWithDerivedObject()
		{
			var x = new RaisingClass();

			var recorded = await Record.ExceptionAsync(() =>
				Assert.RaisesAsync<object>(
					h => x.Completed += h,
					h => x.Completed -= h,
					() => Task.Run(() => x.RaiseWithArgs(new DerivedObject()))
				)
			);

			var exception = Assert.IsType<RaisesException>(recorded);
			Assert.Equal("DerivedObject", exception.Actual);
			Assert.Equal("Object", exception.Expected);
			Assert.Equal("(Raised event did not match expected event)", exception.UserMessage);
		}

		[Fact]
		public static async Task GotExpectedEvent()
		{
			var x = new RaisingClass();
			var genericObject = new object();

			var evt = await Assert.RaisesAsync<object>(
				h => x.Completed += h,
				h => x.Completed -= h,
				() => Task.Run(() => x.RaiseWithArgs(genericObject))
			);

			Assert.NotNull(evt);
			Assert.Equal(x, evt.Sender);
			Assert.Equal(genericObject, evt.Arguments);
		}

	}

	private class RaisingClass
	{
		public void RaiseWithArgs(object args)
		{
			Completed!.Invoke(this, args);
		}

		public event EventHandler<object>? Completed;
	}

	private class DerivedObject : object { }
}
