#if !NET35

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Xml.Linq;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IExecutionVisitor"/> which records the
    /// execution summary for an assembly, as well as performing the XML aggregation
    /// duties of <see cref="XmlTestExecutionVisitor"/>.
    /// </summary>
    [Obsolete("This class has poor performance; please use DelegatingExecutionSummarySink and/or DelegatingXmlCreationSink instead.")]
    public class XmlAggregateVisitor : XmlTestExecutionVisitor, IExecutionVisitor
    {
        readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages;
        readonly IMessageSink innerMessageSink;

        /// <summary>
        /// Initializes a new instance of <see cref="XmlAggregateVisitor"/>.
        /// </summary>
        /// <param name="innerMessageSink">The inner message sink to pass messages to.</param>
        /// <param name="completionMessages">The dictionary which collects execution summaries for all assemblies.</param>
        /// <param name="assemblyElement">The root XML assembly element to collect the result XML.</param>
        /// <param name="cancelThunk">The callback used to determine when to cancel execution.</param>
        public XmlAggregateVisitor(IMessageSink innerMessageSink,
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
        public ExecutionSummary ExecutionSummary { get; private set; }

        /// <inheritdoc/>
        protected override bool Visit(ITestAssemblyFinished assemblyFinished)
        {
            var result = base.Visit(assemblyFinished);

            if (completionMessages != null)
            {
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

            return result;
        }

        /// <inheritdoc/>
        public override bool OnMessage(IMessageSinkMessage message)
        {
            var result = base.OnMessage(message);
            result = innerMessageSink.OnMessage(message) || result;
            return result;
        }
    }
}

#endif
