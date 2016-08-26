﻿#if !NET35

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
    public class XmlAggregateSink :  XmlTestExecutionSink
    {
        readonly IMessageSinkWithTypes innerMessageSink;

        /// <summary>
        /// Initializes a new instance of <see cref="XmlAggregateVisitor"/>.
        /// </summary>
        /// <param name="innerMessageSink">The inner message sink to pass messages to.</param>
        /// <param name="completionMessages">The dictionary which collects execution summaries for all assemblies.</param>
        /// <param name="assemblyElement">The root XML assembly element to collect the result XML.</param>
        /// <param name="cancelThunk">The callback used to determine when to cancel execution.</param>
        /// <param name="longRunningSeconds">Timeout value for a test to be considered "long running"</param>
        public XmlAggregateSink(IMessageSinkWithTypes innerMessageSink,
                                ConcurrentDictionary<string, ExecutionSummary> completionMessages,
                                XElement assemblyElement,
                                Func<bool> cancelThunk,
                                int longRunningSeconds)
            : base(assemblyElement, completionMessages, cancelThunk, longRunningSeconds)
        {
            Guard.ArgumentNotNull(nameof(innerMessageSink), innerMessageSink);

            this.innerMessageSink = innerMessageSink;
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
