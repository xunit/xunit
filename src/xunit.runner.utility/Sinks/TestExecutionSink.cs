#if !NET35


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    ///     An implementation of <see cref="IExecutionSink" /> which records the
    ///     execution summary for an assembly
    /// </summary>
    public class TestExecutionSink : TestMessageSink, IExecutionSink
    {
        readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages;
        int errors;

        /// <summary>
        ///     Initializes a new instance of <see cref="TestExecutionSink" />
        /// </summary>
        /// <param name="completionMessages">The dictionary which collects execution summaries for all assemblies.</param>
        /// <param name="cancelThunk">The callback used to determine when to cancel execution.</param>
        public TestExecutionSink(ConcurrentDictionary<string, ExecutionSummary> completionMessages, Func<bool> cancelThunk)
        {
            this.completionMessages = completionMessages;
            CancelThunk = cancelThunk ?? (() => false);

            ExecutionSummary = new ExecutionSummary();

            TestAssemblyFinishedEvent += OnTestAssemblyFinishedEvent;

            // Hook up error listeners
            TestAssemblyCleanupFailureEvent += delegate { OnError(); };
            TestCaseCleanupFailureEvent += delegate { OnError(); };
            TestClassCleanupFailureEvent += delegate { OnError(); };
            TestCleanupFailureEvent += delegate { OnError(); };
            TestCollectionCleanupFailureEvent += delegate { OnError(); };
            TestMethodCleanupFailureEvent += delegate { OnError(); };
            ErrorMessageEvent += delegate { OnError(); };
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="TestExecutionSink" />
        /// </summary>
        /// <param name="longRunningSeconds">Timeout value for a test to be considered "long running"</param>
        /// <param name="completionMessages">The dictionary which collects execution summaries for all assemblies.</param>
        /// <param name="cancelThunk">The callback used to determine when to cancel execution.</param>
        public TestExecutionSink(ConcurrentDictionary<string, ExecutionSummary> completionMessages, Func<bool> cancelThunk, int longRunningSeconds)
            : this(completionMessages, cancelThunk)
        {
        }

        /// <summary>
        ///     Gets the callback used to determine when to cancel execution.
        /// </summary>
        public Func<bool> CancelThunk { get; }

        /// <inheritdoc />
        public ManualResetEvent Finished { get; } = new ManualResetEvent(initialState: false);

        /// <inheritdoc />
        public ExecutionSummary ExecutionSummary { get; private set; }


        /// <inheritdoc />
        public override bool OnMessageWithTypes(IMessageSinkMessage message, string[] messageTypes)
        {
            var result = base.OnMessageWithTypes(message, messageTypes);
            if (result)
                result = !CancelThunk();

            return result;
        }

        void OnError()
        {
            Interlocked.Increment(ref errors);
        }

        void OnTestAssemblyFinishedEvent(MessageHandlerArgs<ITestAssemblyFinished> args)
        {
            if (completionMessages != null)
            {
                var assemblyFinished = args.Message;
                ExecutionSummary = new ExecutionSummary
                {
                    Total = assemblyFinished.TestsRun,
                    Failed = assemblyFinished.TestsFailed,
                    Skipped = assemblyFinished.TestsSkipped,
                    Time = assemblyFinished.ExecutionTime,
                    Errors = errors
                };

                var assemblyDisplayName = Path.GetFileNameWithoutExtension(assemblyFinished.TestAssembly.Assembly.AssemblyPath);
                completionMessages.TryAdd(assemblyDisplayName, ExecutionSummary);
            }
            Finished.Set();
        }
    }
}

#endif