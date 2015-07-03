using System;
using System.Collections.Concurrent;
using System.IO;
using System.Xml.Linq;
using Xunit.Abstractions;

namespace Xunit
{
    public class XmlAggregateVisitor : XmlTestExecutionVisitor
    {
        readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages;
        readonly IMessageSink innerMessageSink;

        public XmlAggregateVisitor(IMessageSink innerMessageSink,
                                   ConcurrentDictionary<string, ExecutionSummary> completionMessages,
                                   XElement assemblyElement,
                                   Func<bool> cancelThunk)
            : base(assemblyElement, cancelThunk)
        {
            this.innerMessageSink = innerMessageSink;
            this.completionMessages = completionMessages;

            ExecutionSummary = new ExecutionSummary();
        }

        public ExecutionSummary ExecutionSummary { get; private set; }

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

                completionMessages.TryAdd(Path.GetFileNameWithoutExtension(assemblyFinished.TestAssembly.Assembly.AssemblyPath), ExecutionSummary);
            }

            return result;
        }

        public override bool OnMessage(IMessageSinkMessage message)
        {
            var result = base.OnMessage(message);
            result = innerMessageSink.OnMessage(message) || result;
            return result;
        }
    }
}
