using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// A delegating implementation of <see cref="IExecutionSink"/> which detects and reports when
    /// tests have become long-running (during otherwise idle time).
    /// </summary>
    public class DelegatingLongRunningTestDetectionSink : LongLivedMarshalByRefObject, IExecutionSink
    {
        static readonly string[] DiagnosticMessageTypes = { typeof(IDiagnosticMessage).FullName };

        readonly Action<LongRunningTestsSummary> callback;
        readonly Dictionary<ITestCase, DateTime> executingTestCases = new Dictionary<ITestCase, DateTime>();
        readonly ExecutionEventSink executionSink = new ExecutionEventSink();
        readonly IExecutionSink innerSink;
        DateTime lastTestActivity;
        readonly TimeSpan longRunningTestTime;
        ManualResetEvent stopEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatingLongRunningTestDetectionSink"/> class, with
        /// long running test messages being delivered as <see cref="IDiagnosticMessage"/> instances to the
        /// provided diagnostic message sink.
        /// </summary>
        /// <param name="innerSink">The inner sink to delegate to.</param>
        /// <param name="longRunningTestTime">The minimum amount of time a test runs to be considered long running.</param>
        /// <param name="diagnosticMessageSink">The message sink to send messages to.</param>
        public DelegatingLongRunningTestDetectionSink(IExecutionSink innerSink,
                                                      TimeSpan longRunningTestTime,
                                                      IMessageSinkWithTypes diagnosticMessageSink)
            : this(innerSink, longRunningTestTime, summary => DispatchLongRunningTestsSummaryAsDiagnosticMessage(summary, diagnosticMessageSink))
        {
            Guard.ArgumentNotNull(nameof(diagnosticMessageSink), diagnosticMessageSink);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatingLongRunningTestDetectionSink"/> class, with
        /// long running test messages being delivered as <see cref="LongRunningTestsSummary"/> to the
        /// provided callback.
        /// </summary>
        /// <param name="innerSink">The inner sink to delegate to.</param>
        /// <param name="longRunningTestTime">The minimum amount of time a test runs to be considered long running.</param>
        /// <param name="callback">The callback to dispatch messages to.</param>
        public DelegatingLongRunningTestDetectionSink(IExecutionSink innerSink,
                                                      TimeSpan longRunningTestTime,
                                                      Action<LongRunningTestsSummary> callback)
        {
            Guard.ArgumentNotNull(nameof(innerSink), innerSink);
            Guard.ArgumentValid(nameof(longRunningTestTime), "Long running test time must be at least 1 second", longRunningTestTime >= TimeSpan.FromSeconds(1));
            Guard.ArgumentNotNull(nameof(callback), callback);

            this.innerSink = innerSink;
            this.longRunningTestTime = longRunningTestTime;
            this.callback = callback;

            executionSink.TestAssemblyFinishedEvent += HandleTestAssemblyFinished;
            executionSink.TestAssemblyStartingEvent += HandleTestAssemblyStarting;
            executionSink.TestCaseFinishedEvent += HandleTestCaseFinished;
            executionSink.TestCaseStartingEvent += HandleTestCaseStarting;
        }

        /// <inheritdoc/>
        public ExecutionSummary ExecutionSummary => innerSink.ExecutionSummary;

        /// <inheritdoc/>
        public ManualResetEvent Finished => innerSink.Finished;

        /// <summary>
        /// Returns the current time in UTC. Overrideable for testing purposes.
        /// </summary>
        protected virtual DateTime UtcNow => DateTime.UtcNow;

        static void DispatchLongRunningTestsSummaryAsDiagnosticMessage(LongRunningTestsSummary summary, IMessageSinkWithTypes diagnosticMessageSink)
        {
            var messages = summary.TestCases.Select(pair => $"[Long Running Test] '{pair.Key.DisplayName}', Elapsed: {pair.Value:hh\\:mm\\:ss}");
            var message = string.Join(Environment.NewLine, messages.ToArray());

            diagnosticMessageSink.OnMessage(new DiagnosticMessage(message));
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            ((IDisposable)stopEvent).SafeDispose();
            innerSink.Dispose();
        }

        void HandleTestAssemblyFinished(MessageHandlerArgs<ITestAssemblyFinished> args)
        {
            stopEvent.Set();
        }

        void HandleTestAssemblyStarting(MessageHandlerArgs<ITestAssemblyStarting> args)
        {
            stopEvent = new ManualResetEvent(initialState: false);
            lastTestActivity = UtcNow;
            XunitWorkerThread.QueueUserWorkItem(ThreadWorker);
        }

        void HandleTestCaseFinished(MessageHandlerArgs<ITestCaseFinished> args)
        {
            lock (executingTestCases)
            {
                executingTestCases.Remove(args.Message.TestCase);
                lastTestActivity = UtcNow;
            }
        }

        void HandleTestCaseStarting(MessageHandlerArgs<ITestCaseStarting> args)
        {
            lock (executingTestCases)
                executingTestCases.Add(args.Message.TestCase, UtcNow);
        }

        /// <inheritdoc/>
        public bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> messageTypes)
        {
            var result = executionSink.OnMessageWithTypes(message, messageTypes);
            result = innerSink.OnMessageWithTypes(message, messageTypes) && result;
            return result;
        }

        void SendLongRunningMessage()
        {
            Dictionary<ITestCase, TimeSpan> longRunningTestCases;
            lock (executingTestCases)
            {
                var now = UtcNow;
                longRunningTestCases = executingTestCases.Where(kvp => (now - kvp.Value) >= longRunningTestTime)
                                                         .ToDictionary(k => k.Key, v => now - v.Value);
            }

            if (longRunningTestCases.Count > 0)
                callback(new LongRunningTestsSummary(longRunningTestTime, longRunningTestCases));
        }

        void ThreadWorker()
        {
            // Fire the loop approximately every 1/10th of our delay time, but no more frequently than every
            // second (so we don't over-fire the timer). This should give us reasonable precision for the
            // requested delay time, without going crazy to check for long-running tests.

            var delayTime = (int)Math.Max(1000, longRunningTestTime.TotalMilliseconds / 10);

            while (true)
            {
                if (WaitForStopEvent(delayTime))
                    return;

                var now = UtcNow;
                if (now - lastTestActivity >= longRunningTestTime)
                {
                    SendLongRunningMessage();
                    lastTestActivity = now;
                }
            }
        }

        /// <summary>
        /// Performs a Task-safe delay. Overrideable for testing purposes.
        /// </summary>
        protected virtual bool WaitForStopEvent(int millionsecondsDelay)
            => stopEvent.WaitOne(millionsecondsDelay);

    }
}
