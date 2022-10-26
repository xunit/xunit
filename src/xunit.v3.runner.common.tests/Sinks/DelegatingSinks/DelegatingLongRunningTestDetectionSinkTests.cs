using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Runner.Common;
using Xunit.v3;

public class DelegatingLongRunningTestDetectionSinkTests
{
	[Fact]
	public async ValueTask ShortRunningTests_NoMessages()
	{
		var events = new List<LongRunningTestsSummary>();

		using var sink = new TestableDelegatingLongRunningTestDetectionSink(longRunningSeconds: 1, callback: summary => events.Add(summary));

		sink.OnMessage(new _TestAssemblyStarting());
		sink.OnMessage(new _TestCaseStarting { TestCaseUniqueID = "test-case-id" });
		await sink.AdvanceClockAsync(100);
		sink.OnMessage(new _TestCaseFinished { TestCaseUniqueID = "test-case-id" });
		sink.OnMessage(new _TestAssemblyFinished());

		Assert.Empty(events);
	}

	[Fact]
	public async ValueTask LongRunningTest_Once_WithCallback()
	{
		var events = new List<LongRunningTestsSummary>();

		using var sink = new TestableDelegatingLongRunningTestDetectionSink(longRunningSeconds: 1, callback: summary => events.Add(summary));
		var testCaseStarting = new _TestCaseStarting { TestCaseUniqueID = "test-case-id" };

		sink.OnMessage(new _TestAssemblyStarting());
		sink.OnMessage(testCaseStarting);
		await sink.AdvanceClockAsync(1500);
		sink.OnMessage(new _TestCaseFinished { TestCaseUniqueID = "test-case-id" });
		sink.OnMessage(new _TestAssemblyFinished());

		var @event = Assert.Single(events);
		Assert.Equal(TimeSpan.FromSeconds(1), @event.ConfiguredLongRunningTime);
		var receivedTestCasePair = Assert.Single(@event.TestCases);
		Assert.Same(testCaseStarting, receivedTestCasePair.Key);
		Assert.Equal(TimeSpan.FromMilliseconds(1500), receivedTestCasePair.Value);
	}

	[Fact]
	public async ValueTask OnlyIncludesLongRunningTests()
	{
		var events = new List<LongRunningTestsSummary>();

		using var sink = new TestableDelegatingLongRunningTestDetectionSink(longRunningSeconds: 1, callback: summary => events.Add(summary));
		var testCaseStarting1 = new _TestCaseStarting { TestCaseUniqueID = "test-case-id-1" };
		var testCaseStarting2 = new _TestCaseStarting { TestCaseUniqueID = "test-case-id-2" };

		sink.OnMessage(new _TestAssemblyStarting());
		sink.OnMessage(testCaseStarting1);
		await sink.AdvanceClockAsync(500);
		sink.OnMessage(testCaseStarting2);  // Started later, hasn't run long enough
		await sink.AdvanceClockAsync(500);
		sink.OnMessage(new _TestCaseFinished { TestCaseUniqueID = "test-case-id-1" });
		sink.OnMessage(new _TestCaseFinished { TestCaseUniqueID = "test-case-id-2" });
		sink.OnMessage(new _TestAssemblyFinished());

		var @event = Assert.Single(events);
		Assert.Equal(TimeSpan.FromSeconds(1), @event.ConfiguredLongRunningTime);
		var receivedTestCasePair = Assert.Single(@event.TestCases);
		Assert.Same(testCaseStarting1, receivedTestCasePair.Key);
		Assert.Equal(TimeSpan.FromSeconds(1), receivedTestCasePair.Value);
	}

	[Fact]
	public async ValueTask LongRunningTest_Twice_WithCallback()
	{
		var events = new List<LongRunningTestsSummary>();

		using var sink = new TestableDelegatingLongRunningTestDetectionSink(longRunningSeconds: 1, callback: summary => events.Add(summary));
		var testCaseStarting = new _TestCaseStarting { TestCaseUniqueID = "test-case-id" };

		sink.OnMessage(new _TestAssemblyStarting());
		sink.OnMessage(testCaseStarting);
		await sink.AdvanceClockAsync(1000);
		await sink.AdvanceClockAsync(500);
		await sink.AdvanceClockAsync(500);
		sink.OnMessage(new _TestCaseFinished { TestCaseUniqueID = "test-case-id" });
		sink.OnMessage(new _TestAssemblyFinished());

		Assert.Collection(
			events,
			@event =>
			{
				Assert.Equal(TimeSpan.FromSeconds(1), @event.ConfiguredLongRunningTime);
				var receivedTestCasePair = Assert.Single(@event.TestCases);
				Assert.Same(testCaseStarting, receivedTestCasePair.Key);
				Assert.Equal(TimeSpan.FromSeconds(1), receivedTestCasePair.Value);
			},
			@event =>
			{
				Assert.Equal(TimeSpan.FromSeconds(1), @event.ConfiguredLongRunningTime);
				var receivedTestCasePair = Assert.Single(@event.TestCases);
				Assert.Same(testCaseStarting, receivedTestCasePair.Key);
				Assert.Equal(TimeSpan.FromSeconds(2), receivedTestCasePair.Value);
			}
		);
	}

	[Fact]
	public async ValueTask LongRunningTest_Once_WithDiagnosticMessageSink()
	{
		var events = new List<_DiagnosticMessage>();
		var diagSink = Substitute.For<_IMessageSink>();
		diagSink
			.WhenForAnyArgs(x => x.OnMessage(null!))
			.Do(callInfo =>
			{
				var message = callInfo.Arg<_MessageSinkMessage>();
				if (message is _DiagnosticMessage diagnosticMessage)
					events.Add(diagnosticMessage);
			});

		using var sink = new TestableDelegatingLongRunningTestDetectionSink(longRunningSeconds: 1, diagnosticMessageSink: diagSink);
		var testCaseStarting = new _TestCaseStarting { TestCaseDisplayName = "My test display name", TestCaseUniqueID = "test-case-id" };

		sink.OnMessage(new _TestAssemblyStarting());
		sink.OnMessage(testCaseStarting);
		await sink.AdvanceClockAsync(1500);
		sink.OnMessage(new _TestCaseFinished { TestCaseUniqueID = "test-case-id" });
		sink.OnMessage(new _TestAssemblyFinished());

		var @event = Assert.Single(events);
		Assert.Equal("Long running test: 'My test display name' (elapsed: 00:00:01)", @event.Message);
	}

	[Fact]
	public async ValueTask LongRunningTest_Twice_WithDiagnosticMessageSink()
	{
		var events = new List<_DiagnosticMessage>();
		var diagSink = Substitute.For<_IMessageSink>();
		diagSink
			.WhenForAnyArgs(x => x.OnMessage(null!))
			.Do(callInfo =>
			{
				var message = callInfo.Arg<_MessageSinkMessage>();
				if (message is _DiagnosticMessage diagnosticMessage)
					events.Add(diagnosticMessage);
			});

		using var sink = new TestableDelegatingLongRunningTestDetectionSink(longRunningSeconds: 1, diagnosticMessageSink: diagSink);
		var testCaseStarting = new _TestCaseStarting { TestCaseDisplayName = "My test display name", TestCaseUniqueID = "test-case-id" };

		sink.OnMessage(new _TestAssemblyStarting());
		sink.OnMessage(testCaseStarting);
		await sink.AdvanceClockAsync(1000);
		await sink.AdvanceClockAsync(500);
		await sink.AdvanceClockAsync(500);
		sink.OnMessage(new _TestCaseFinished { TestCaseUniqueID = "test-case-id" });
		sink.OnMessage(new _TestAssemblyFinished());

		Assert.Collection(
			events,
			@event => Assert.Equal("Long running test: 'My test display name' (elapsed: 00:00:01)", @event.Message),
			@event => Assert.Equal("Long running test: 'My test display name' (elapsed: 00:00:02)", @event.Message)
		);
	}

	class TestableDelegatingLongRunningTestDetectionSink : DelegatingLongRunningTestDetectionSink
	{
		volatile bool stop = false;
		volatile int stopEventTriggerCount;
		DateTime utcNow = DateTime.UtcNow;
		readonly AutoResetEvent workEvent = new(initialState: false);

		public TestableDelegatingLongRunningTestDetectionSink(
			int longRunningSeconds,
			_IMessageSink diagnosticMessageSink)
				: base(Substitute.For<IExecutionSink>(), TimeSpan.FromSeconds(longRunningSeconds), diagnosticMessageSink)
		{ }

		public TestableDelegatingLongRunningTestDetectionSink(
			int longRunningSeconds,
			Action<LongRunningTestsSummary>? callback = null)
				: base(Substitute.For<IExecutionSink>(), TimeSpan.FromSeconds(longRunningSeconds), callback ?? (_ => { }))
		{ }

		protected override DateTime UtcNow => utcNow;

		public async Task AdvanceClockAsync(int milliseconds)
		{
			utcNow += TimeSpan.FromMilliseconds(milliseconds);

			var currentCount = stopEventTriggerCount;
			workEvent.Set();

			var stopTime = DateTime.UtcNow.AddSeconds(60);

			while (stopTime > DateTime.UtcNow)
			{
				await Task.Delay(25);
				if (currentCount != stopEventTriggerCount)
					return;
			}

			throw new InvalidOperationException("After AdvanceClock, next work run never happened.");
		}

		public override void Dispose()
		{
			stop = true;
			workEvent.Set();

			var stopTime = DateTime.UtcNow.AddSeconds(60);

			while (stopTime > DateTime.UtcNow)
			{
				Thread.Sleep(25);
				if (stopEventTriggerCount == -1)
				{
					workEvent.Dispose();
					return;
				}
			}

			throw new InvalidOperationException("Worker thread did not shut down within 60 seconds.");
		}

		protected override bool WaitForStopEvent(int millionsecondsDelay)
		{
			Interlocked.Increment(ref stopEventTriggerCount);

			workEvent.WaitOne();

			if (stop)
			{
				stopEventTriggerCount = -1;
				return true;
			}

			return false;
		}
	}
}
