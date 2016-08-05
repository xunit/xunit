#if !NET35

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IExecutionVisitor"/> which records the
    /// execution summary for an assembly, as well as performing the XML aggregation
    /// duties of <see cref="XmlTestExecutionSink"/>.
    /// </summary>
    public class XmlAggregateSink : XmlTestExecutionSink, IExecutionSink
    {
        readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages;
        readonly IMessageSinkWithTypes innerMessageSink;

        /// <summary>
        /// Initializes a new instance of <see cref="XmlAggregateVisitor"/>.
        /// </summary>
        /// <param name="innerMessageSink">The inner message sink to pass messages to.</param>
        /// <param name="completionMessages">The dictionary which collects execution summaries for all assemblies.</param>
        /// <param name="assemblyElement">The root XML assembly element to collect the result XML.</param>
        /// <param name="cancelThunk">The callback used to determine when to cancel execution.</param>
        public XmlAggregateSink(IMessageSinkWithTypes innerMessageSink,
                                ConcurrentDictionary<string, ExecutionSummary> completionMessages,
                                XElement assemblyElement,
                                Func<bool> cancelThunk)
            : base(assemblyElement, cancelThunk)
        {
            Guard.ArgumentNotNull(nameof(innerMessageSink), innerMessageSink);

            this.innerMessageSink = innerMessageSink;
            this.completionMessages = completionMessages;

            ExecutionSummary = new ExecutionSummary();
        }

        /// <inheritdoc/>
        public ManualResetEvent Finished { get; } = new ManualResetEvent(initialState: false);

        /// <inheritdoc/>
        public ExecutionSummary ExecutionSummary { get; private set; }

        /// <inheritdoc/>
        protected override void HandleTestAssemblyFinished(MessageHandlerArgs<ITestAssemblyFinished> args)
        {
            base.HandleTestAssemblyFinished(args);

            if (completionMessages != null)
            {
                var assemblyFinished = args.Message;
                ExecutionSummary = new ExecutionSummary
                {
                    Total = assemblyFinished.TestsRun,
                    Failed = assemblyFinished.TestsFailed,
                    Skipped = assemblyFinished.TestsSkipped,
                    Time = assemblyFinished.ExecutionTime,
                    Errors = Errors
                };

                var assemblyDisplayName = Path.GetFileNameWithoutExtension(assemblyFinished.TestAssembly.Assembly.AssemblyPath);
                completionMessages.TryAdd(assemblyDisplayName, ExecutionSummary);
            }

            Finished.Set();
        }

        /// <inheritdoc/>
        public override bool OnMessageWithTypes(IMessageSinkMessage message, string[] messageTypes)
        {
            var result = base.OnMessageWithTypes(message, messageTypes);

            return innerMessageSink.OnMessageWithTypes(message, messageTypes) || result;
        }
    }
}

#endif
