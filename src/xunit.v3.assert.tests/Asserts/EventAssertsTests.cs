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

			var recorded = Record.Exception(() => Assert.Raises<EventArgs>(h => x.Completed += h, h => x.Completed -= h, () => { }));

			var exception = Assert.IsType<RaisesException>(recorded);
			Assert.Equal("(No event was raised)", exception.Actual);
			Assert.Equal("EventArgs", exception.Expected);
		}

		[Fact]
		public static void ExpectedEventButRaisesEventWithDerivedArgs()
		{
			var x = new RaisingClass();

			var recorded = Record.Exception(() =>
				Assert.Raises<EventArgs>(
					h => x.Completed += h,
					h => x.Completed -= h,
					() => x.RaiseWithArgs(new DerivedEventArgs())
				)
			);

			var exception = Assert.IsType<RaisesException>(recorded);
			Assert.Equal("DerivedEventArgs", exception.Actual);
			Assert.Equal("EventArgs", exception.Expected);
			Assert.Equal("(Raised event did not match expected event)", exception.UserMessage);
		}

		[Fact]
		public static void GotExpectedEvent()
		{
			var x = new RaisingClass();

			var evt = Assert.Raises<EventArgs>(
				h => x.Completed += h,
				h => x.Completed -= h,
				() => x.RaiseWithArgs(EventArgs.Empty)
			);

			Assert.NotNull(evt);
			Assert.Equal(x, evt.Sender);
			Assert.Equal(EventArgs.Empty, evt.Arguments);
		}
	}

	public class RaisesAny_Generic
	{
		[Fact]
		public static void ExpectedEventButCodeDoesNotRaise()
		{
			var x = new RaisingClass();

			var recorded = Record.Exception(() => Assert.RaisesAny<EventArgs>(h => x.Completed += h, h => x.Completed -= h, () => { }));

			var exception = Assert.IsType<RaisesException>(recorded);
			Assert.Equal("(No event was raised)", exception.Actual);
			Assert.Equal("EventArgs", exception.Expected);
		}

		[Fact]
		public static void GotExpectedEvent()
		{
			var x = new RaisingClass();

			var evt = Assert.RaisesAny<EventArgs>(
				h => x.Completed += h,
				h => x.Completed -= h,
				() => x.RaiseWithArgs(EventArgs.Empty)
			);

			Assert.NotNull(evt);
			Assert.Equal(x, evt.Sender);
			Assert.Equal(EventArgs.Empty, evt.Arguments);
		}


		[Fact]
		public static void GotDerivedEvent()
		{
			var x = new RaisingClass();
			var args = new DerivedEventArgs();

			var evt = Assert.RaisesAny<EventArgs>(
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
				() => Assert.RaisesAnyAsync<EventArgs>(
					h => x.Completed += h, h => x.Completed -= h, () => Task.Run(() => { })
				)
			);

			var exception = Assert.IsType<RaisesException>(recorded);
			Assert.Equal("(No event was raised)", exception.Actual);
			Assert.Equal("EventArgs", exception.Expected);
		}

		[Fact]
		public static async Task GotExpectedEvent()
		{
			var x = new RaisingClass();

			var evt = await Assert.RaisesAnyAsync<EventArgs>(
				h => x.Completed += h,
				h => x.Completed -= h,
				() => Task.Run(() => x.RaiseWithArgs(EventArgs.Empty))
			);

			Assert.NotNull(evt);
			Assert.Equal(x, evt.Sender);
			Assert.Equal(EventArgs.Empty, evt.Arguments);
		}

		[Fact]
		public static async Task GotDerivedEvent()
		{
			var x = new RaisingClass();
			var args = new DerivedEventArgs();

			var evt = await Assert.RaisesAnyAsync<EventArgs>(
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
				() => Assert.RaisesAsync<EventArgs>(
					h => x.Completed += h, h => x.Completed -= h, () => Task.Run(() => { })
				)
			);

			var exception = Assert.IsType<RaisesException>(recorded);
			Assert.Equal("(No event was raised)", exception.Actual);
			Assert.Equal("EventArgs", exception.Expected);
		}

		[Fact]
		public static async Task ExpectedEventButRaisesEventWithDerivedArgs()
		{
			var x = new RaisingClass();

			var recorded = await Record.ExceptionAsync(() =>
				Assert.RaisesAsync<EventArgs>(
					h => x.Completed += h,
					h => x.Completed -= h,
					() => Task.Run(() => x.RaiseWithArgs(new DerivedEventArgs()))
				)
			);

			var exception = Assert.IsType<RaisesException>(recorded);
			Assert.Equal("DerivedEventArgs", exception.Actual);
			Assert.Equal("EventArgs", exception.Expected);
			Assert.Equal("(Raised event did not match expected event)", exception.UserMessage);
		}

		[Fact]
		public static async Task GotExpectedEvent()
		{
			var x = new RaisingClass();

			var evt = await Assert.RaisesAsync<EventArgs>(
				h => x.Completed += h,
				h => x.Completed -= h,
				() => Task.Run(() => x.RaiseWithArgs(EventArgs.Empty))
			);

			Assert.NotNull(evt);
			Assert.Equal(x, evt.Sender);
			Assert.Equal(EventArgs.Empty, evt.Arguments);
		}
	}

	private class RaisingClass
	{
		public void RaiseWithArgs(EventArgs args)
		{
			Completed!.Invoke(this, args);
		}

		public event EventHandler<EventArgs>? Completed;
	}

	private class DerivedEventArgs : EventArgs { }
}
