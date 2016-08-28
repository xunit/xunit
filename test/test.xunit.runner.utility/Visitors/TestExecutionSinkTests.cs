using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public class TestExecutionSinkTests
{
    public class OnMessage_TestAssemblyFinished
    {
        [Fact]
        public void SetsExecutionSummary()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            assemblyFinished.TestsRun.Returns(2112);
            assemblyFinished.TestsFailed.Returns(42);
            assemblyFinished.TestsSkipped.Returns(6);
            assemblyFinished.ExecutionTime.Returns(123.4567M);

            var sink = new TestableTestExecutionSink();

            sink.OnMessage(assemblyFinished);

            Assert.Equal(2112, sink.ExecutionSummary.Total);
            Assert.Equal(42, sink.ExecutionSummary.Failed);
            Assert.Equal(6, sink.ExecutionSummary.Skipped);
            Assert.Equal(123.4567M, sink.ExecutionSummary.Time);
        }
    }

    public class LongRunningTestDetection
    {
        [Fact]
        public async void ShortRunningTests_NoMessages()
        {
            var sink = new TestableTestExecutionSink(longRunningSeconds: 1);
            var events = new List<ILongRunningTestsMessage>();
            sink.LongRunningTestsEvent += args => events.Add(args.Message);
            var testCase1 = Substitute.For<ITestCase>();

            sink.OnMessage(Substitute.For<ITestAssemblyStarting>());
            sink.OnMessage(new TestCaseStarting(testCase1));
            await sink.AdvanceClockAsync(100);
            sink.OnMessage(new TestCaseFinished(testCase1, 8009, 1, 0, 0));
            sink.OnMessage(Substitute.For<ITestAssemblyFinished>());

            Assert.Empty(events);
        }

        [Fact]
        public async void LongRunningTest_Once_WithLongRunningTestHandler()
        {
            var sink = new TestableTestExecutionSink(longRunningSeconds: 1);
            var events = new List<ILongRunningTestsMessage>();
            sink.LongRunningTestsEvent += args => events.Add(args.Message);
            var testCase = Substitute.For<ITestCase>();

            sink.OnMessage(Substitute.For<ITestAssemblyStarting>());
            sink.OnMessage(new TestCaseStarting(testCase));
            await sink.AdvanceClockAsync(1000);
            sink.OnMessage(new TestCaseFinished(testCase, 8009, 1, 0, 0));
            sink.OnMessage(Substitute.For<ITestAssemblyFinished>());

            var @event = Assert.Single(events);
            Assert.Equal(TimeSpan.FromSeconds(1), @event.ConfiguredLongRunningTime);
            var receivedTestCasePair = Assert.Single(@event.TestCases);
            Assert.Same(testCase, receivedTestCasePair.Key);
            Assert.Equal(TimeSpan.FromSeconds(1), receivedTestCasePair.Value);
        }

        [Fact]
        public async void OnlyIncludesLongRunningTests()
        {
            var sink = new TestableTestExecutionSink(longRunningSeconds: 1);
            var events = new List<ILongRunningTestsMessage>();
            sink.LongRunningTestsEvent += args => events.Add(args.Message);
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

        [Fact]
        public async void LongRunningTest_Twice_WithLongRunningTestHandler()
        {
            var sink = new TestableTestExecutionSink(longRunningSeconds: 1);
            var events = new List<ILongRunningTestsMessage>();
            sink.LongRunningTestsEvent += args => events.Add(args.Message);
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

        [Fact]
        public async void LongRunningTest_Once_WithoutLongRunningTestHandler()
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
            var sink = new TestableTestExecutionSink(diagnosticMessageSink: diagSink, longRunningSeconds: 1);
            var testCase = Substitute.For<ITestCase>();
            testCase.DisplayName.Returns("My test display name");

            sink.OnMessage(Substitute.For<ITestAssemblyStarting>());
            sink.OnMessage(new TestCaseStarting(testCase));
            await sink.AdvanceClockAsync(1000);
            sink.OnMessage(new TestCaseFinished(testCase, 8009, 1, 0, 0));
            sink.OnMessage(Substitute.For<ITestAssemblyFinished>());

            var @event = Assert.Single(events);
            Assert.Equal("[Long Running Test] 'My test display name', Elapsed: 00:00:01", @event.Message);
        }

        [Fact]
        public async void LongRunningTest_Twice_WithoutLongRunningTestHandler()
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
            var sink = new TestableTestExecutionSink(diagnosticMessageSink: diagSink, longRunningSeconds: 1);
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

    class TestableTestExecutionSink : TestExecutionSink
    {
        volatile TaskCompletionSource<int> delayTask;
        volatile int delayTriggerCount;
        DateTime utcNow = DateTime.UtcNow;

        public TestableTestExecutionSink(IMessageSinkWithTypes diagnosticMessageSink = null,
                                         ConcurrentDictionary<string, ExecutionSummary> competionMessages = null,
                                         Func<bool> cancelThunk = null,
                                         int longRunningSeconds = -1)
            : base(diagnosticMessageSink, competionMessages, cancelThunk, longRunningSeconds) { }

        protected override DateTime UtcNow => utcNow;

        public async Task AdvanceClockAsync(int milliseconds)
        {
            var currentTask = delayTask;

            utcNow += TimeSpan.FromMilliseconds(milliseconds);

            if (currentTask != null)
            {
                var currentCount = delayTriggerCount;

                // Let the background worker do its thing
                currentTask.SetResult(0);

                var stopTime = DateTime.UtcNow.AddSeconds(60);

                while (stopTime > DateTime.UtcNow)
                {
                    await Task.Delay(25);
                    if (currentCount != delayTriggerCount)
                        return;
                }

                throw new InvalidOperationException("After AdvanceClock, next delay was never triggered");
            }
        }

        // Control the delay so that it's only triggered when we advance the clock, and track the fact
        // that a delay has been requested so the clock advance knows when work is complete.
        protected override Task DelayAsync(int millionsecondsDelay, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref delayTriggerCount);
            delayTask = new TaskCompletionSource<int>();
            return delayTask.Task;
        }

        public bool OnMessage(IMessageSinkMessage message)
            => ((IMessageSink)this).OnMessage(message);
    }
}
