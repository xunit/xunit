#if !NET35

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IExecutionSink" /> which records the execution summary for
    /// an assembly. It also provides a facility for cancelation, as well as the ability to detect
    /// and report about long-running tests.
    /// </summary>
    public class TestExecutionSink : TestMessageSink, IExecutionSink
    {
        static readonly HashSet<string> DiagnosticMessageTypes = new HashSet<string> { typeof(IDiagnosticMessage).FullName };

        CancellationTokenSource cancellationTokenSource;
        readonly Func<bool> cancelThunk;
        readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages;
        readonly IMessageSinkWithTypes diagnosticMessageSink;
        volatile int errors;
        readonly ConcurrentDictionary<ITestCase, DateTime> executingTestCases;
        DateTime lastTestActivity;
        readonly TimeSpan longRunningTime;
        Task timerTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestExecutionSink"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink to report diagnostic messages to.</param>
        /// <param name="completionMessages">The dictionary which collects execution summaries for all assemblies.</param>
        /// <param name="cancelThunk">The callback used to determine when to cancel execution.</param>
        /// <param name="longRunningSeconds">Timeout value for a test to be considered "long running".</param>
        public TestExecutionSink(IMessageSinkWithTypes diagnosticMessageSink,
                                 ConcurrentDictionary<string, ExecutionSummary> completionMessages,
                                 Func<bool> cancelThunk,
                                 int longRunningSeconds)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
            this.completionMessages = completionMessages;
            this.cancelThunk = cancelThunk ?? (() => false);

            TestAssemblyFinishedEvent += args =>
            {
                // We fill in ExecutionSummary here before we call the extensibility point, because
                // we want anybody who derives from this to be able to count on the values being
                // there when running the message handler; we also want them to be able to run
                // their code before delegating to our implementation, since we're the ones who
                // are responsible for triggering the Finished event.
                var assemblyFinished = args.Message;

                ExecutionSummary.Total = assemblyFinished.TestsRun;
                ExecutionSummary.Failed = assemblyFinished.TestsFailed;
                ExecutionSummary.Skipped = assemblyFinished.TestsSkipped;
                ExecutionSummary.Time = assemblyFinished.ExecutionTime;
                ExecutionSummary.Errors = errors;

                if (completionMessages != null)
                {
                    var assemblyDisplayName = Path.GetFileNameWithoutExtension(assemblyFinished.TestAssembly.Assembly.AssemblyPath);
                    completionMessages.TryAdd(assemblyDisplayName, ExecutionSummary);
                }

                if (longRunningTime != default(TimeSpan))
                    cancellationTokenSource.Cancel();

                HandleTestAssemblyFinished(args);
            };

            TestAssemblyCleanupFailureEvent += delegate { Interlocked.Increment(ref errors); };
            TestCaseCleanupFailureEvent += delegate { Interlocked.Increment(ref errors); };
            TestClassCleanupFailureEvent += delegate { Interlocked.Increment(ref errors); };
            TestCleanupFailureEvent += delegate { Interlocked.Increment(ref errors); };
            TestCollectionCleanupFailureEvent += delegate { Interlocked.Increment(ref errors); };
            TestMethodCleanupFailureEvent += delegate { Interlocked.Increment(ref errors); };
            ErrorMessageEvent += delegate { Interlocked.Increment(ref errors); };

            if (longRunningSeconds > 0)
            {

                longRunningTime = TimeSpan.FromSeconds(longRunningSeconds);
                executingTestCases = new ConcurrentDictionary<ITestCase, DateTime>();

                // Only need to subscribe these when tracking long-running tests
                TestAssemblyStartingEvent += HandleTestAssemblyStarting;
                TestCaseStartingEvent += HandleTestCaseStarting;
                TestCaseFinishedEvent += HandleTestCaseFinished;
            }
        }

        /// <inheritdoc/>
        public ExecutionSummary ExecutionSummary { get; } = new ExecutionSummary();

        /// <inheritdoc/>
        public ManualResetEvent Finished { get; } = new ManualResetEvent(initialState: false);

        /// <summary>
        /// Occurs when a <see cref="ILongRunningTestsMessage"/> message is received.
        /// </summary>
        public event MessageHandler<ILongRunningTestsMessage> LongRunningTestsEvent;

        /// <summary>
        /// Returns the current time in UTC. Overrideable for testing purposes only.
        /// </summary>
        protected virtual DateTime UtcNow => DateTime.UtcNow;

        /// <summary>
        /// Performs a Task-safe delay. Overrideablke for testing purposes only.
        /// </summary>
        protected virtual Task DelayAsync(int millionsecondsDelay, CancellationToken cancellationToken)
            => Task.Delay(millionsecondsDelay, cancellationToken);

        /// <summary>
        /// Called when an object is no longer needed.
        /// </summary>
        public Task DisposeAsync()
            => timerTask ?? CommonTasks.Completed;

        /// <summary>
        /// Called when <see cref="TestMessageSink.TestAssemblyFinishedEvent"/> is raised.
        /// This signals the <see cref="Finished"/> event, which allows runners to know that
        /// execution is complete for this test assembly. Classes which override this method
        /// should be sure to do their work before calling this base version, if it is
        /// important that their work be done before the <see cref="Finished"/> event is signaled.
        /// </summary>
        protected virtual void HandleTestAssemblyFinished(MessageHandlerArgs<ITestAssemblyFinished> args)
            => Finished.Set();

        void HandleTestAssemblyStarting(MessageHandlerArgs<ITestAssemblyStarting> args)
        {
            cancellationTokenSource = new CancellationTokenSource();
            timerTask = StartTimerAsync();
            lastTestActivity = UtcNow;
        }

        void HandleTestCaseFinished(MessageHandlerArgs<ITestCaseFinished> args)
        {
            DateTime date;
            executingTestCases.TryRemove(args.Message.TestCase, out date);
            lastTestActivity = UtcNow;
        }

        void HandleTestCaseStarting(MessageHandlerArgs<ITestCaseStarting> args)
            => executingTestCases.TryAdd(args.Message.TestCase, UtcNow);

        /// <inheritdoc/>
        public override bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> messageTypes)
            => base.OnMessageWithTypes(message, messageTypes) && !cancelThunk();

        void SendLongRunningMessage()
        {
            var now = UtcNow;
            var longRunningTestCases = executingTestCases.Where(kvp => (now - kvp.Value) >= longRunningTime)
                                                         .ToDictionary(k => k.Key, v => now - v.Value);

            if (longRunningTestCases.Count > 0)
            {
                // If they've subscribed for long running test messages, just give them one...
                if (LongRunningTestsEvent != null)
                    LongRunningTestsEvent.Invoke(new MessageHandlerArgs<ILongRunningTestsMessage>(new LongRunningTestsMessage(longRunningTime, longRunningTestCases)));

                // ...otherwise, turn it into a diagnostic message, for older runners.
                else
                {
                    var messages = longRunningTestCases.Select(pair => $"[Long Running Test] '{pair.Key.DisplayName}', Elapsed: {pair.Value:hh\\:mm\\:ss}");
                    var message = string.Join(Environment.NewLine, messages);

                    diagnosticMessageSink?.OnMessageWithTypes(new DiagnosticMessage(message), DiagnosticMessageTypes);
                }
            }
        }

        async Task StartTimerAsync()
        {
            // Fire the loop approximately every 1/10th of our delay time, but no more frequently than every
            // second (so we don't over-fire the timer). This should give us reasonable precision for the
            // requested delay time, without going crazy to check for long-running tests.

            var delayTime = (int)Math.Max(1000, longRunningTime.TotalMilliseconds / 10);

            while (true)
            {
                await DelayAsync(delayTime, cancellationTokenSource.Token).ConfigureAwait(false);
                if (cancellationTokenSource.IsCancellationRequested)
                    return;

                var now = UtcNow;
                if (now - lastTestActivity >= longRunningTime)
                {
                    SendLongRunningMessage();
                    lastTestActivity = now;
                }
            }
        }
    }
}

#endif
