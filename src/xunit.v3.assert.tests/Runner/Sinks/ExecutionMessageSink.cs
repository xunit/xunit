using System;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit
{
    public class ExecutionMessageSink : IMessageSink
    {
        readonly Func<bool> cancelThunk;
        volatile int errors;
        readonly IMessageSink innerSink;

        public ExecutionMessageSink(IMessageSink innerSink, Func<bool> cancelThunk = null)
        {
            this.innerSink = innerSink;
            this.cancelThunk = cancelThunk ?? (() => false);
        }

        /// <summary>
        /// Gets the final execution summary, once the execution is finished.
        /// </summary>
        public ExecutionSummary ExecutionSummary { get; } = new ExecutionSummary();

        /// <summary>
        /// Gets an event which is signaled once execution is finished.
        /// </summary>
        public ManualResetEvent Finished { get; } = new ManualResetEvent(initialState: false);

        void HandleTestAssemblyFinished(ITestAssemblyFinished testAssemblyFinishedMessage)
        {
            ExecutionSummary.Total = testAssemblyFinishedMessage.TestsRun;
            ExecutionSummary.Failed = testAssemblyFinishedMessage.TestsFailed;
            ExecutionSummary.Skipped = testAssemblyFinishedMessage.TestsSkipped;
            ExecutionSummary.Time = testAssemblyFinishedMessage.ExecutionTime;
            ExecutionSummary.Errors = errors;

            Finished.Set();
        }

        public bool OnMessage(IMessageSinkMessage message)
        {
            var result = innerSink.OnMessage(message);

            if (message is IErrorMessage ||
                message is ITestAssemblyCleanupFailure ||
                message is ITestCaseCleanupFailure ||
                message is ITestClassCleanupFailure ||
                message is ITestCleanupFailure ||
                message is ITestCollectionCleanupFailure ||
                message is ITestMethodCleanupFailure)
            {
                Interlocked.Increment(ref errors);
            }

            if (message is ITestAssemblyFinished testAssemblyFinishedMessage)
                HandleTestAssemblyFinished(testAssemblyFinishedMessage);

            return result && !cancelThunk();
        }
    }
}
