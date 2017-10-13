using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public class DelegatingLongRunningTestDetectionSinkTests
{
    [Fact]
    public async void ShortRunningTests_NoMessages()
    {
        var events = new List<LongRunningTestsSummary>();

        using (var sink = new TestableDelegatingLongRunningTestDetectionSink(longRunningSeconds: 1, callback: summary => events.Add(summary)))
        {
            var testCase1 = Substitute.For<ITestCase>();

            sink.OnMessage(Substitute.For<ITestAssemblyStarting>());
            sink.OnMessage(new TestCaseStarting(testCase1));
            await sink.AdvanceClockAsync(100);
            sink.OnMessage(new TestCaseFinished(testCase1, 8009, 1, 0, 0));
            sink.OnMessage(Substitute.For<ITestAssemblyFinished>());

            Assert.Empty(events);
        }
    }

    [Fact]
    public async void LongRunningTest_Once_WithCallback()
    {
        var events = new List<LongRunningTestsSummary>();

        using (var sink = new TestableDelegatingLongRunningTestDetectionSink(longRunningSeconds: 1, callback: summary => events.Add(summary)))
        {
            var testCase = Substitute.For<ITestCase>();

            sink.OnMessage(Substitute.For<ITestAssemblyStarting>());
            sink.OnMessage(new TestCaseStarting(testCase));
            await sink.AdvanceClockAsync(1500);
            sink.OnMessage(new TestCaseFinished(testCase, 8009, 1, 0, 0));
            sink.OnMessage(Substitute.For<ITestAssemblyFinished>());

            var @event = Assert.Single(events);
            Assert.Equal(TimeSpan.FromSeconds(1), @event.ConfiguredLongRunningTime);
            var receivedTestCasePair = Assert.Single(@event.TestCases);
            Assert.Same(testCase, receivedTestCasePair.Key);
            Assert.Equal(TimeSpan.FromMilliseconds(1500), receivedTestCasePair.Value);
        }
    }

    [Fact]
    public async void OnlyIncludesLongRunningTests()
    {
        var events = new List<LongRunningTestsSummary>();

        using (var sink = new TestableDelegatingLongRunningTestDetectionSink(longRunningSeconds: 1, callback: summary => events.Add(summary)))
        {
            var testCase1 = Substitute.For<ITestCase>();
            var testCase2 = Substitute.For<ITestCase>();

            sink.OnMessage(Substitute.For<ITestAssemblyStarting>());
            sink.OnMessage(new TestCaseStarting(testCase1));
            await sink.AdvanceClockAsync(500);
            sink.OnMessage(new TestCaseStarting(testCase2));  // Started later, hasn't run long enough
            await sink.AdvanceClockAsync(500);
            sink.OnMessage(new TestCaseFinished(testCase1, 8009, 1, 0, 0));
            sink.OnMessage(new TestCaseFinished(testCase2, 8009, 1, 0, 0));
            sink.OnMessage(Substitute.For<ITestAssemblyFinished>());

            var @event = Assert.Single(events);
            Assert.Equal(TimeSpan.FromSeconds(1), @event.ConfiguredLongRunningTime);
            var receivedTestCasePair = Assert.Single(@event.TestCases);
            Assert.Same(testCase1, receivedTestCasePair.Key);
            Assert.Equal(TimeSpan.FromSeconds(1), receivedTestCasePair.Value);
        }
    }

    [Fact]
    public async void LongRunningTest_Twice_WithCallback()
    {
        var events = new List<LongRunningTestsSummary>();

        using (var sink = new TestableDelegatingLongRunningTestDetectionSink(longRunningSeconds: 1, callback: summary => events.Add(summary)))
        {
            var testCase = Substitute.For<ITestCase>();

            sink.OnMessage(Substitute.For<ITestAssemblyStarting>());
            sink.OnMessage(new TestCaseStarting(testCase));
            await sink.AdvanceClockAsync(1000);
            await sink.AdvanceClockAsync(500);
            await sink.AdvanceClockAsync(500);
            sink.OnMessage(new TestCaseFinished(testCase, 8009, 1, 0, 0));
            sink.OnMessage(Substitute.For<ITestAssemblyFinished>());

            Assert.Collection(events,
                @event =>
                {
                    Assert.Equal(TimeSpan.FromSeconds(1), @event.ConfiguredLongRunningTime);
                    var receivedTestCasePair = Assert.Single(@event.TestCases);
                    Assert.Same(testCase, receivedTestCasePair.Key);
                    Assert.Equal(TimeSpan.FromSeconds(1), receivedTestCasePair.Value);
                },
                @event =>
                {
                    Assert.Equal(TimeSpan.FromSeconds(1), @event.ConfiguredLongRunningTime);
                    var receivedTestCasePair = Assert.Single(@event.TestCases);
                    Assert.Same(testCase, receivedTestCasePair.Key);
                    Assert.Equal(TimeSpan.FromSeconds(2), receivedTestCasePair.Value);
                }
            );
        }
    }

    [Fact]
    public async void LongRunningTest_Once_WithDiagnosticMessageSink()
    {
        var events = new List<IDiagnosticMessage>();
        var diagSink = Substitute.For<IMessageSinkWithTypes>();
        diagSink.WhenForAnyArgs(x => x.OnMessageWithTypes(null, null))
                .Do(callInfo =>
                {
                    var message = callInfo.Arg<IMessageSinkMessage>();
                    var diagnosticMessage = message as IDiagnosticMessage;
                    if (diagnosticMessage != null)
                        events.Add(diagnosticMessage);
                });

        using (var sink = new TestableDelegatingLongRunningTestDetectionSink(longRunningSeconds: 1, diagnosticMessageSink: diagSink))
        {
            var testCase = Substitute.For<ITestCase>();
            testCase.DisplayName.Returns("My test display name");

            sink.OnMessage(Substitute.For<ITestAssemblyStarting>());
            sink.OnMessage(new TestCaseStarting(testCase));
            await sink.AdvanceClockAsync(1500);
            sink.OnMessage(new TestCaseFinished(testCase, 8009, 1, 0, 0));
            sink.OnMessage(Substitute.For<ITestAssemblyFinished>());

            var @event = Assert.Single(events);
            Assert.Equal("[Long Running Test] 'My test display name', Elapsed: 00:00:01", @event.Message);
        }
    }

    [Fact]
    public async void LongRunningTest_Twice_WithDiagnosticMessageSink()
    {
        var events = new List<IDiagnosticMessage>();
        var diagSink = Substitute.For<IMessageSinkWithTypes>();
        diagSink.WhenForAnyArgs(x => x.OnMessageWithTypes(null, null))
                .Do(callInfo =>
                {
                    var message = callInfo.Arg<IMessageSinkMessage>();
                    var diagnosticMessage = message as IDiagnosticMessage;
                    if (diagnosticMessage != null)
                        events.Add(diagnosticMessage);
                });

        using (var sink = new TestableDelegatingLongRunningTestDetectionSink(longRunningSeconds: 1, diagnosticMessageSink: diagSink))
        {
            var testCase = Substitute.For<ITestCase>();
            testCase.DisplayName.Returns("My test display name");

            sink.OnMessage(Substitute.For<ITestAssemblyStarting>());
            sink.OnMessage(new TestCaseStarting(testCase));
            await sink.AdvanceClockAsync(1000);
            await sink.AdvanceClockAsync(500);
            await sink.AdvanceClockAsync(500);
            sink.OnMessage(new TestCaseFinished(testCase, 8009, 1, 0, 0));
            sink.OnMessage(Substitute.For<ITestAssemblyFinished>());

            Assert.Collection(events,
                @event => Assert.Equal("[Long Running Test] 'My test display name', Elapsed: 00:00:01", @event.Message),
                @event => Assert.Equal("[Long Running Test] 'My test display name', Elapsed: 00:00:02", @event.Message)
            );
        }
    }

    class TestableDelegatingLongRunningTestDetectionSink : DelegatingLongRunningTestDetectionSink
    {
        volatile bool stop = false;
        volatile int stopEventTriggerCount;
        DateTime utcNow = DateTime.UtcNow;
        AutoResetEvent workEvent = new AutoResetEvent(initialState: false);

        public TestableDelegatingLongRunningTestDetectionSink(int longRunningSeconds, IMessageSinkWithTypes diagnosticMessageSink)
            : base(Substitute.For<IExecutionSink>(), TimeSpan.FromSeconds(longRunningSeconds), diagnosticMessageSink) { }

        public TestableDelegatingLongRunningTestDetectionSink(int longRunningSeconds, Action<LongRunningTestsSummary> callback = null)
            : base(Substitute.For<IExecutionSink>(), TimeSpan.FromSeconds(longRunningSeconds), callback ?? (_ => { })) { }

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

        public bool OnMessage(IMessageSinkMessage message)
            => OnMessageWithTypes(message, null);
    }
}
