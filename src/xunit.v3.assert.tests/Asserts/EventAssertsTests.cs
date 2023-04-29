using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class EventAssertsTests
{
	public class Raises
	{
		[Fact]
		public static void NoEventRaised()
		{
			var obj = new RaisingClass();

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
			var obj = new RaisingClass();
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
			var obj = new RaisingClass();
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

	public class RaisesAny
	{
		[Fact]
		public static void NoEventRaised()
		{
			var obj = new RaisingClass();

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
			var obj = new RaisingClass();
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
		public static void DerivedTypeRaised()
		{
			var obj = new RaisingClass();
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
	}

	public class RaisesAnyAsync_Task
	{
		[Fact]
		public static async Task NoEventRaised()
		{
			var obj = new RaisingClass();

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
			var obj = new RaisingClass();
			var eventObj = new object();

			var evt = await Assert.RaisesAnyAsync<object>(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() => Task.Run(() => obj.RaiseWithArgs(eventObj))
			);

			Assert.NotNull(evt);
			Assert.Equal(obj, evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}

		[Fact]
		public static async Task DerivedTypeRaised()
		{
			var obj = new RaisingClass();
			var eventObj = new DerivedObject();

			var evt = await Assert.RaisesAnyAsync<object>(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() => Task.Run(() => obj.RaiseWithArgs(eventObj))
			);

			Assert.NotNull(evt);
			Assert.Equal(obj, evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}
	}

#if XUNIT_VALUETASK
	public class RaisesAnyAsync_ValueTask
	{
		[Fact]
		public static async ValueTask NoEventRaised()
		{
			var obj = new RaisingClass();

			var ex = await Record.ExceptionAsync(
				() => Assert.RaisesAnyAsync<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => default(ValueTask)
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
		public static async ValueTask ExactTypeRaised()
		{
			var obj = new RaisingClass();
			var eventObj = new object();

			var evt = await Assert.RaisesAnyAsync<object>(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() =>
				{
					obj.RaiseWithArgs(eventObj);
					return default(ValueTask);
				}
			);

			Assert.NotNull(evt);
			Assert.Equal(obj, evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}

		[Fact]
		public static async ValueTask DerivedTypeRaised()
		{
			var obj = new RaisingClass();
			var eventObj = new DerivedObject();

			var evt = await Assert.RaisesAnyAsync<object>(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() =>
				{
					obj.RaiseWithArgs(eventObj);
					return default(ValueTask);
				}
			);

			Assert.NotNull(evt);
			Assert.Equal(obj, evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}
	}
#endif

	public class RaisesAsync_Task
	{
		[Fact]
		public static async Task NoEventRaised()
		{
			var obj = new RaisingClass();

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
			var obj = new RaisingClass();
			var eventObj = new object();

			var evt = await Assert.RaisesAsync<object>(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() => Task.Run(() => obj.RaiseWithArgs(eventObj))
			);

			Assert.NotNull(evt);
			Assert.Equal(obj, evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}

		[Fact]
		public static async Task DerivedTypeRaised()
		{
			var obj = new RaisingClass();
			var eventObj = new DerivedObject();

			var ex = await Record.ExceptionAsync(
				() => Assert.RaisesAsync<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => Task.Run(() => obj.RaiseWithArgs(eventObj))
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

#if XUNIT_VALUETASK
	public class RaisesAsync_ValueTask
	{
		[Fact]
		public static async ValueTask NoEventRaised()
		{
			var obj = new RaisingClass();

			var ex = await Record.ExceptionAsync(
				() => Assert.RaisesAsync<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() => default(ValueTask)
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
		public static async ValueTask ExactTypeRaised()
		{
			var obj = new RaisingClass();
			var eventObj = new object();

			var evt = await Assert.RaisesAsync<object>(
				h => obj.Completed += h,
				h => obj.Completed -= h,
				() =>
				{
					obj.RaiseWithArgs(eventObj);
					return default(ValueTask);
				}
			);

			Assert.NotNull(evt);
			Assert.Equal(obj, evt.Sender);
			Assert.Equal(eventObj, evt.Arguments);
		}

		[Fact]
		public static async ValueTask DerivedTypeRaised()
		{
			var obj = new RaisingClass();
			var eventObj = new DerivedObject();

			var ex = await Record.ExceptionAsync(
				() => Assert.RaisesAsync<object>(
					h => obj.Completed += h,
					h => obj.Completed -= h,
					() =>
					{
						obj.RaiseWithArgs(eventObj);
						return default(ValueTask);
					}
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
#endif

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
