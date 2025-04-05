using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class EventAssertsTests
{
	public class Raises_Action
	{
		[Fact]
		public static void NoEventRaised()
		{
			var obj = new RaisingClass_ActionOfT();

			var ex = Record.Exception(
				() => Assert.Raises<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => { }
				)
			);

			Assert.IsType<RaisesException>(ex);
			Assert.Equal(
				"Assert.Raises() Failure: No event was raised" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				"Actual:   No event was raised",
				ex.Message
			);
		}

		[Fact]
		public static void NoEventRaised_NoData()
		{
			var obj = new RaisingClass_Action();

			var ex = Record.Exception(
				() => Assert.Raises(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => { }
				)
			);

			Assert.IsType<RaisesException>(ex);
			Assert.Equal("Assert.Raises() Failure: No event was raised", ex.Message);
		}

		[Fact]
		public static void ExactTypeRaised()
		{
			var obj = new RaisingClass_ActionOfT();
			var eventObj = new object();

			var evt = Assert.Raises<object>(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() => obj.RaiseWithArgs(eventObj)
			);

			Assert.NotNull(evt);
			Assert.Null(evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}

		[Fact]
		public static void RaisingClass_ActionOfT()
		{
			var obj = new RaisingClass_EventHandlerOfT();
			var eventObj = new DerivedObject();

			var ex = Record.Exception(
				() => Assert.Raises<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => obj.RaiseWithArgs(eventObj)
				)
			);

			Assert.IsType<RaisesException>(ex);
			Assert.Equal(
				"Assert.Raises() Failure: Wrong event type was raised" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				$"Actual:   typeof({typeof(DerivedObject).FullName})",
				ex.Message
			);
		}
	}

	public class Raises_EventHandler
	{
		[Fact]
		public static void NoEventRaised()
		{
			var obj = new RaisingClass_EventHandlerOfT();

			var ex = Record.Exception(
				() => Assert.Raises<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => { }
				)
			);

			Assert.IsType<RaisesException>(ex);
			Assert.Equal(
				"Assert.Raises() Failure: No event was raised" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				"Actual:   No event was raised",
				ex.Message
			);
		}

		[Fact]
		public static void ExactTypeRaised()
		{
			var obj = new RaisingClass_EventHandlerOfT();
			var eventObj = new object();

			var evt = Assert.Raises<object>(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() => obj.RaiseWithArgs(eventObj)
			);

			Assert.NotNull(evt);
			Assert.Equal(obj, evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}

		[Fact]
		public static void DerivedTypeRaised()
		{
			var obj = new RaisingClass_EventHandlerOfT();
			var eventObj = new DerivedObject();

			var ex = Record.Exception(
				() => Assert.Raises<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => obj.RaiseWithArgs(eventObj)
				)
			);

			Assert.IsType<RaisesException>(ex);
			Assert.Equal(
				"Assert.Raises() Failure: Wrong event type was raised" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				$"Actual:   typeof({typeof(DerivedObject).FullName})",
				ex.Message
			);
		}

		[Fact]
		public static void CustomRaised()
		{
			var obj = new RaisingClass_CustomEventHandler();
			var eventObj = new object();
			Assert.RaisedEvent<object>? raisedEvent = null;
			void handler(object? s, object args) => raisedEvent = new Assert.RaisedEvent<object>(s, args);

			var evt = Assert.Raises(
				() => raisedEvent,
				() => obj.Completed += handler,
				() => obj.Completed -= handler,
				() => obj.RaiseWithArgs(eventObj)
			);

			Assert.NotNull(evt);
			Assert.Equal(obj, evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}
	}

	public class RaisesAny_Action
	{
		[Fact]
		public static void NoEventRaised()
		{
			var obj = new RaisingClass_ActionOfT();

			var ex = Record.Exception(
				() => Assert.RaisesAny<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => { }
				)
			);

			Assert.IsType<RaisesAnyException>(ex);
			Assert.Equal(
				"Assert.RaisesAny() Failure: No event was raised" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				"Actual:   No event was raised",
				ex.Message
			);
		}

		[Fact]
		public static void ExactTypeRaised()
		{
			var obj = new RaisingClass_ActionOfT();
			var eventObj = new object();

			var evt = Assert.RaisesAny<object>(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() => obj.RaiseWithArgs(eventObj)
			);

			Assert.NotNull(evt);
			Assert.Null(evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}

		[Fact]
		public static void DerivedTypeRaised()
		{
			var obj = new RaisingClass_ActionOfT();
			var eventObj = new DerivedObject();

			var evt = Assert.RaisesAny<object>(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() => obj.RaiseWithArgs(eventObj)
			);

			Assert.NotNull(evt);
			Assert.Null(evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}
	}

	public class RaisesAny_EventHandler
	{
		[Fact]
		public static void NoEventRaised()
		{
			var obj = new RaisingClass_EventHandlerOfT();

			var ex = Record.Exception(
				() => Assert.RaisesAny<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => { }
				)
			);

			Assert.IsType<RaisesAnyException>(ex);
			Assert.Equal(
				"Assert.RaisesAny() Failure: No event was raised" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				"Actual:   No event was raised",
				ex.Message
			);
		}

		[Fact]
		public static void NoEventRaised_NonGeneric()
		{
			var obj = new RaisingClass_EventHandler();

			var ex = Record.Exception(
				() => Assert.RaisesAny(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => { }
				)
			);

			Assert.IsType<RaisesAnyException>(ex);
			Assert.Equal(
				"Assert.RaisesAny() Failure: No event was raised" + Environment.NewLine +
				"Expected: typeof(System.EventArgs)" + Environment.NewLine +
				"Actual:   No event was raised",
				ex.Message
			);
		}

		[Fact]
		public static void ExactTypeRaised()
		{
			var obj = new RaisingClass_EventHandlerOfT();
			var eventObj = new object();

			var evt = Assert.RaisesAny<object>(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() => obj.RaiseWithArgs(eventObj)
			);

			Assert.NotNull(evt);
			Assert.Equal(obj, evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}

		[Fact]
		public static void ExactTypeRaised_NonGeneric()
		{
			var obj = new RaisingClass_EventHandler();
			var eventObj = new EventArgs();

			var evt = Assert.RaisesAny(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() => obj.RaiseWithArgs(eventObj)
			);

			Assert.NotNull(evt);
			Assert.Equal(obj, evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}

		[Fact]
		public static void DerivedTypeRaised()
		{
			var obj = new RaisingClass_EventHandlerOfT();
			var eventObj = new DerivedObject();

			var evt = Assert.RaisesAny<object>(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() => obj.RaiseWithArgs(eventObj)
			);

			Assert.NotNull(evt);
			Assert.Equal(obj, evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}

		[Fact]
		public static void DerivedTypeRaised_NonGeneric()
		{
			var obj = new RaisingClass_EventHandler();
			var eventObj = new DerivedEventArgs();

			var evt = Assert.RaisesAny(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() => obj.RaiseWithArgs(eventObj)
			);

			Assert.NotNull(evt);
			Assert.Equal(obj, evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}
	}

	public class RaisesAnyAsync_Action
	{
		[Fact]
		public static async Task NoEventRaised()
		{
			var obj = new RaisingClass_ActionOfT();

			var ex = await Record.ExceptionAsync(
				() => Assert.RaisesAnyAsync<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => Task.FromResult(0)
				)
			);

			Assert.IsType<RaisesAnyException>(ex);
			Assert.Equal(
				"Assert.RaisesAny() Failure: No event was raised" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				"Actual:   No event was raised",
				ex.Message
			);
		}

		[Fact]
		public static async Task ExactTypeRaised()
		{
			var obj = new RaisingClass_ActionOfT();
			var eventObj = new object();

			var evt = await Assert.RaisesAnyAsync<object>(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() => Task.Run(() => obj.RaiseWithArgs(eventObj), TestContext.Current.CancellationToken)
			);

			Assert.NotNull(evt);
			Assert.Null(evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}

		[Fact]
		public static async Task DerivedTypeRaised()
		{
			var obj = new RaisingClass_ActionOfT();
			var eventObj = new DerivedObject();

			var evt = await Assert.RaisesAnyAsync<object>(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() => Task.Run(() => obj.RaiseWithArgs(eventObj), TestContext.Current.CancellationToken)
			);

			Assert.NotNull(evt);
			Assert.Null(evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}
	}

	public class RaisesAnyAsync_EventHandler
	{
		[Fact]
		public static async Task NoEventRaised()
		{
			var obj = new RaisingClass_EventHandlerOfT();

			var ex = await Record.ExceptionAsync(
				() => Assert.RaisesAnyAsync<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => Task.FromResult(0)
				)
			);

			Assert.IsType<RaisesAnyException>(ex);
			Assert.Equal(
				"Assert.RaisesAny() Failure: No event was raised" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				"Actual:   No event was raised",
				ex.Message
			);
		}

		[Fact]
		public static async Task NoEventRaised_NonGeneric()
		{
			var obj = new RaisingClass_EventHandler();

			var ex = await Record.ExceptionAsync(
				() => Assert.RaisesAnyAsync(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => Task.FromResult(0)
				)
			);

			Assert.IsType<RaisesAnyException>(ex);
			Assert.Equal(
				"Assert.RaisesAny() Failure: No event was raised" + Environment.NewLine +
				"Expected: typeof(System.EventArgs)" + Environment.NewLine +
				"Actual:   No event was raised",
				ex.Message
			);
		}

		[Fact]
		public static async Task ExactTypeRaised()
		{
			var obj = new RaisingClass_EventHandlerOfT();
			var eventObj = new object();

			var evt = await Assert.RaisesAnyAsync<object>(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() => Task.Run(() => obj.RaiseWithArgs(eventObj), TestContext.Current.CancellationToken)
			);

			Assert.NotNull(evt);
			Assert.Equal(obj, evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}

		[Fact]
		public static async Task ExactTypeRaised_NonGeneric()
		{
			var obj = new RaisingClass_EventHandler();
			var eventObj = new EventArgs();

			var evt = await Assert.RaisesAnyAsync(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() => Task.Run(() => obj.RaiseWithArgs(eventObj), TestContext.Current.CancellationToken)
			);

			Assert.NotNull(evt);
			Assert.Equal(obj, evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}

		[Fact]
		public static async Task DerivedTypeRaised()
		{
			var obj = new RaisingClass_EventHandlerOfT();
			var eventObj = new DerivedObject();

			var evt = await Assert.RaisesAnyAsync<object>(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() => Task.Run(() => obj.RaiseWithArgs(eventObj), TestContext.Current.CancellationToken)
			);

			Assert.NotNull(evt);
			Assert.Equal(obj, evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}

		[Fact]
		public static async Task DerivedTypeRaised_NonGeneric()
		{
			var obj = new RaisingClass_EventHandler();
			var eventObj = new DerivedEventArgs();

			var evt = await Assert.RaisesAnyAsync(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() => Task.Run(() => obj.RaiseWithArgs(eventObj), TestContext.Current.CancellationToken)
			);

			Assert.NotNull(evt);
			Assert.Equal(obj, evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}
	}

	public class RaisesAsync_Action
	{
		[Fact]
		public static async Task NoEventRaised()
		{
			var obj = new RaisingClass_ActionOfT();

			var ex = await Record.ExceptionAsync(
				() => Assert.RaisesAsync<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => Task.FromResult(0)
				)
			);

			Assert.IsType<RaisesException>(ex);
			Assert.Equal(
				"Assert.Raises() Failure: No event was raised" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				"Actual:   No event was raised",
				ex.Message
			);
		}

		[Fact]
		public static async Task NoEventRaised_NoData()
		{
			var obj = new RaisingClass_Action();

			var ex = await Record.ExceptionAsync(
				() => Assert.RaisesAsync(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => Task.CompletedTask
				)
			);

			Assert.IsType<RaisesException>(ex);
			Assert.Equal("Assert.Raises() Failure: No event was raised", ex.Message);
		}

		[Fact]
		public static async Task ExactTypeRaised()
		{
			var obj = new RaisingClass_ActionOfT();
			var eventObj = new object();

			var evt = await Assert.RaisesAsync<object>(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() => Task.Run(() => obj.RaiseWithArgs(eventObj), TestContext.Current.CancellationToken)
			);

			Assert.NotNull(evt);
			Assert.Null(evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}

		[Fact]
		public static async Task DerivedTypeRaised()
		{
			var obj = new RaisingClass_ActionOfT();
			var eventObj = new DerivedObject();

			var ex = await Record.ExceptionAsync(
				() => Assert.RaisesAsync<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => Task.Run(() => obj.RaiseWithArgs(eventObj), TestContext.Current.CancellationToken)
				)
			);

			Assert.IsType<RaisesException>(ex);
			Assert.Equal(
				"Assert.Raises() Failure: Wrong event type was raised" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				$"Actual:   typeof({typeof(DerivedObject).FullName})",
				ex.Message
			);
		}
	}

	public class RaisesAsync_EventHandler
	{
		[Fact]
		public static async Task NoEventRaised()
		{
			var obj = new RaisingClass_EventHandlerOfT();

			var ex = await Record.ExceptionAsync(
				() => Assert.RaisesAsync<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => Task.FromResult(0)
				)
			);

			Assert.IsType<RaisesException>(ex);
			Assert.Equal(
				"Assert.Raises() Failure: No event was raised" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				"Actual:   No event was raised",
				ex.Message
			);
		}

		[Fact]
		public static async Task ExactTypeRaised()
		{
			var obj = new RaisingClass_EventHandlerOfT();
			var eventObj = new object();

			var evt = await Assert.RaisesAsync<object>(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() => Task.Run(() => obj.RaiseWithArgs(eventObj), TestContext.Current.CancellationToken)
			);

			Assert.NotNull(evt);
			Assert.Equal(obj, evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}

		[Fact]
		public static async Task DerivedTypeRaised()
		{
			var obj = new RaisingClass_EventHandlerOfT();
			var eventObj = new DerivedObject();

			var ex = await Record.ExceptionAsync(
				() => Assert.RaisesAsync<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => Task.Run(() => obj.RaiseWithArgs(eventObj), TestContext.Current.CancellationToken)
				)
			);

			Assert.IsType<RaisesException>(ex);
			Assert.Equal(
				"Assert.Raises() Failure: Wrong event type was raised" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				$"Actual:   typeof({typeof(DerivedObject).FullName})",
				ex.Message
			);
		}
	}

	public class NotRaisedAny_Action
	{
		[Fact]
		public static void NoEventRaised()
		{
			var obj = new RaisingClass_ActionOfT();

			var ex = Record.Exception(
				() => Assert.NotRaisedAny<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => { }
				)
			);

			Assert.Null(ex);
		}

		[Fact]
		public static void EventRaised()
		{
			var obj = new RaisingClass_ActionOfT();
			var eventObj = new object();

			var ex = Record.Exception(
				() => Assert.NotRaisedAny<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => obj.RaiseWithArgs(eventObj)
				)
			);

			Assert.IsType<NotRaisesException>(ex);
			Assert.Equal(
				$"Assert.NotRaisedAny() Failure: An unexpected event was raised{Environment.NewLine}Unexpected: typeof(object){Environment.NewLine}Actual:   An event was raised",
				ex.Message
			);
		}

	}

	public class NotRaisedAny_EventHandler
	{
		[Fact]
		public static void NoEventRaised()
		{
			var obj = new RaisingClass_EventHandlerOfT();

			var ex = Record.Exception(
				() => Assert.NotRaisedAny<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => { }
				)
			);

			Assert.Null(ex);
		}

		[Fact]
		public static void EventRaised()
		{
			var obj = new RaisingClass_EventHandlerOfT();
			var eventObj = new object();

			var ex = Record.Exception(
				() => Assert.NotRaisedAny<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => obj.RaiseWithArgs(eventObj)
				)
			);

			Assert.IsType<NotRaisesException>(ex);
			Assert.Equal(
				$"Assert.NotRaisedAny() Failure: An unexpected event was raised{Environment.NewLine}Unexpected: typeof(object){Environment.NewLine}Actual:   An event was raised",
				ex.Message
			);
		}
	}

	public class NotRaisedAnyAsync_Action
	{
		[Fact]
		public static async Task NoEventRaised()
		{
			var obj = new RaisingClass_ActionOfT();

			var ex = await Record.ExceptionAsync(
				() => Assert.NotRaisedAnyAsync<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => Task.FromResult(0)
				)
			);

			Assert.Null(ex);
		}

		[Fact]
		public static async Task EventRaised()
		{
			var obj = new RaisingClass_ActionOfT();
			var eventObj = new object();

			var ex = await Record.ExceptionAsync(
				() => Assert.NotRaisedAnyAsync<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => Task.Run(() => obj.RaiseWithArgs(eventObj), TestContext.Current.CancellationToken)
				)
			);

			Assert.IsType<NotRaisesException>(ex);
			Assert.Equal(
				$"Assert.NotRaisedAny() Failure: An unexpected event was raised{Environment.NewLine}Unexpected: typeof(object){Environment.NewLine}Actual:   An event was raised",
				ex.Message
			);
		}
	}

	public class NotRaisedAnyAsync_EventHandler
	{
		[Fact]
		public static async Task NoEventRaised()
		{
			var obj = new RaisingClass_EventHandlerOfT();

			var ex = await Record.ExceptionAsync(
				() => Assert.NotRaisedAnyAsync<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => Task.FromResult(0)
				)
			);

			Assert.Null(ex);
		}

		[Fact]
		public static async Task EventRaised()
		{
			var obj = new RaisingClass_EventHandlerOfT();
			var eventObj = new object();

			var ex = await Record.ExceptionAsync(
				() => Assert.NotRaisedAnyAsync<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => Task.Run(() => obj.RaiseWithArgs(eventObj), TestContext.Current.CancellationToken)
				)
			);

			Assert.IsType<NotRaisesException>(ex);
			Assert.Equal(
				$"Assert.NotRaisedAny() Failure: An unexpected event was raised{Environment.NewLine}Unexpected: typeof(object){Environment.NewLine}Actual:   An event was raised",
				ex.Message
			);
		}
	}

	public class NotRaisedAny_NoArgs
	{
		[Fact]
		public static void NoEventRaised()
		{
			var obj = new RaisingClass_Action();

			var ex = Record.Exception(
				() => Assert.NotRaisedAny(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => { }
				)
			);

			Assert.Null(ex);
		}

		[Fact]
		public static void EventRaised()
		{
			var obj = new RaisingClass_Action();

			var ex = Record.Exception(
				() => Assert.NotRaisedAny(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => obj.Raise()
				)
			);

			Assert.IsType<NotRaisesException>(ex);
			Assert.Equal("Assert.NotRaisedAny() Failure: An unexpected event was raised", ex.Message);
		}
	}

	public class NotRaisedAnyAsync_NoArgs
	{
		[Fact]
		public static async Task NoEventRaised()
		{
			var obj = new RaisingClass_Action();

			var ex = await Record.ExceptionAsync(
				() => Assert.NotRaisedAnyAsync(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => Task.CompletedTask
				)
			);

			Assert.Null(ex);
		}

		[Fact]
		public static async Task EventRaised()
		{
			var obj = new RaisingClass_Action();

			var ex = await Record.ExceptionAsync(
				() => Assert.NotRaisedAnyAsync(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => Task.Run(() => obj.Raise(), TestContext.Current.CancellationToken)
				)
			);

			Assert.IsType<NotRaisesException>(ex);
			Assert.Equal("Assert.NotRaisedAny() Failure: An unexpected event was raised", ex.Message);
		}
	}

	class RaisingClass_Action
	{
		public void Raise()
		{
			Completed!.Invoke();
		}

		public event Action? Completed;
	}

	class RaisingClass_ActionOfT
	{
		public void RaiseWithArgs(object args)
		{
			Completed!.Invoke(args);
		}

		public event Action<object>? Completed;
	}

	class RaisingClass_EventHandler
	{
		public void RaiseWithArgs(EventArgs args)
		{
			Completed!.Invoke(this, args);
		}

		public event EventHandler? Completed;
	}

	class RaisingClass_EventHandlerOfT
	{
		public void RaiseWithArgs(object args)
		{
			Completed!.Invoke(this, args);
		}

		public event EventHandler<object>? Completed;
	}

	class RaisingClass_CustomEventHandler
	{
		public void RaiseWithArgs(object args)
		{
			Completed!.Invoke(this, args);
		}

		public event CustomEventHandler<object>? Completed;
	}

	class DerivedEventArgs : EventArgs { }

	class DerivedObject : object { }

	delegate void CustomEventHandler<TEventArgs>(object sender, TEventArgs e);
}
