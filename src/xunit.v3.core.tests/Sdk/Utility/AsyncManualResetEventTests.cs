using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class AsyncManualResetEventTests
{
	public class WaitAsync
	{
		[Fact]
		public void NonSignaledEvent_ReturnsIncompleteValueTask()
		{
			var evt = new AsyncManualResetEvent(signaled: false);

			var task = evt.WaitAsync();

			Assert.False(task.IsCompleted);
		}

		[Fact]
		public void SignaledEvent_ReturnsCompletedValueTask()
		{
			var evt = new AsyncManualResetEvent(signaled: true);

			var task = evt.WaitAsync();

			Assert.True(task.IsCompleted);
			Assert.False(task.IsFaulted);
			Assert.True(task.IsCompletedSuccessfully);
			Assert.False(task.IsFaulted);
		}

		[Fact]
		public void TaskSignalsAfterEventIsSet()
		{
			var evt = new AsyncManualResetEvent(signaled: false);

			var task = evt.WaitAsync();

			Assert.False(task.IsCompleted);

			evt.Set();

			Assert.True(task.IsCompleted);
		}

		[Fact]
		public void TaskUnsignalsAfterEventIsReset()
		{
			var evt = new AsyncManualResetEvent(signaled: true);

			var task = evt.WaitAsync();

			Assert.True(task.IsCompleted);

			evt.Reset();

			Assert.False(task.IsCompleted);
		}

		[Fact]
		public async ValueTask SignaledEventCanBeAwaited()
		{
			var evt = new AsyncManualResetEvent(signaled: false);
			var delayTask = Task.Delay(10000);
			var eventTask = evt.WaitAsync().AsTask();
			await Task.Delay(1);

			evt.Set();
			var result = await Task.WhenAny(delayTask, eventTask);

			Assert.Same(eventTask, result);
		}

		[Fact]
		public async ValueTask UnsignaledEventWaitsForeverForSignal()
		{
			var evt = new AsyncManualResetEvent(signaled: false);
			var delayTask = Task.Delay(100);
			var eventTask = evt.WaitAsync().AsTask();
			await Task.Delay(1);

			var result = await Task.WhenAny(delayTask, eventTask);

			Assert.Same(delayTask, result);
		}
	}
}
