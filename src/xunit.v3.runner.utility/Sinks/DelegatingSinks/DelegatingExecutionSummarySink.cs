using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// A delegating implementation of <see cref="IExecutionSink"/> which provides the execution
    /// summary and finished events when appropriate and cancellation support.
    /// </summary>
    public class DelegatingExecutionSummarySink : LongLivedMarshalByRefObject, IExecutionSink
    {
        readonly Func<bool> cancelThunk;
        readonly Action<string, ExecutionSummary> completionCallback;
        volatile int errors;
        readonly IMessageSinkWithTypes innerSink;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatingExecutionSummarySink"/> class.
        /// </summary>
        /// <param name="innerSink">The inner sink to pass messages to.</param>
        /// <param name="cancelThunk"></param>
        /// <param name="completionCallback"></param>
        public DelegatingExecutionSummarySink(IMessageSinkWithTypes innerSink,
                                              Func<bool> cancelThunk = null,
                                              Action<string, ExecutionSummary> completionCallback = null)
        {
            Guard.ArgumentNotNull(nameof(innerSink), innerSink);

            this.innerSink = innerSink;
            this.cancelThunk = cancelThunk ?? (() => false);
            this.completionCallback = completionCallback;
        }

        /// <inheritdoc/>
        public ExecutionSummary ExecutionSummary { get; } = new ExecutionSummary();

        /// <inheritdoc/>
        public ManualResetEvent Finished { get; } = new ManualResetEvent(initialState: false);

        /// <inheritdoc/>
        public void Dispose()
            => ((IDisposable)Finished).Dispose();

        void HandleTestAssemblyFinished(MessageHandlerArgs<ITestAssemblyFinished> args)
        {
            ExecutionSummary.Total = args.Message.TestsRun;
            ExecutionSummary.Failed = args.Message.TestsFailed;
            ExecutionSummary.Skipped = args.Message.TestsSkipped;
            ExecutionSummary.Time = args.Message.ExecutionTime;
            ExecutionSummary.Errors = errors;

            completionCallback?.Invoke(Path.GetFileNameWithoutExtension(args.Message.TestAssembly.Assembly.AssemblyPath), ExecutionSummary);

            Finished.Set();
        }

        /// <inheritdoc/>
        public bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> messageTypes)
        {
            var result = innerSink.OnMessageWithTypes(message, messageTypes);

            return message.Dispatch<IErrorMessage>(messageTypes, args => Interlocked.Increment(ref errors))
                && message.Dispatch<ITestAssemblyCleanupFailure>(messageTypes, args => Interlocked.Increment(ref errors))
                && message.Dispatch<ITestAssemblyFinished>(messageTypes, HandleTestAssemblyFinished)
                && message.Dispatch<ITestCaseCleanupFailure>(messageTypes, args => Interlocked.Increment(ref errors))
                && message.Dispatch<ITestClassCleanupFailure>(messageTypes, args => Interlocked.Increment(ref errors))
                && message.Dispatch<ITestCleanupFailure>(messageTypes, args => Interlocked.Increment(ref errors))
                && message.Dispatch<ITestCollectionCleanupFailure>(messageTypes, args => Interlocked.Increment(ref errors))
                && message.Dispatch<ITestMethodCleanupFailure>(messageTypes, args => Interlocked.Increment(ref errors))
                && result
                && !cancelThunk();
        }
    }
}
