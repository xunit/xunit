using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Class that maps test runner messages to events.
    /// </summary>
    public class RunnerEventSink : LongLivedMarshalByRefObject, IMessageSinkWithTypes
    {
        /// <summary>
        /// Occurs when the runner is starting discovery for a given test assembly.
        /// </summary>
        public event MessageHandler<ITestAssemblyDiscoveryFinished> TestAssemblyDiscoveryFinishedEvent;

        /// <summary>
        /// Occurs when the runner has finished discovery for a given test assembly.
        /// </summary>
        public event MessageHandler<ITestAssemblyDiscoveryStarting> TestAssemblyDiscoveryStartingEvent;

        /// <summary>
        /// Occurs when the runner has finished executing the given test assembly.
        /// </summary>
        public event MessageHandler<ITestAssemblyExecutionFinished> TestAssemblyExecutionFinishedEvent;

        /// <summary>
        /// Occurs when the runner is starting to execution the given test assembly.
        /// </summary>
        public event MessageHandler<ITestAssemblyExecutionStarting> TestAssemblyExecutionStartingEvent;

        /// <summary>
        /// Occurs when the runner has finished executing all test assemblies.
        /// </summary>
        public event MessageHandler<ITestExecutionSummary> TestExecutionSummaryEvent;

        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
        public bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> messageTypes)
            => message.Dispatch(messageTypes, TestAssemblyDiscoveryFinishedEvent)
            && message.Dispatch(messageTypes, TestAssemblyDiscoveryStartingEvent)
            && message.Dispatch(messageTypes, TestAssemblyExecutionFinishedEvent)
            && message.Dispatch(messageTypes, TestAssemblyExecutionStartingEvent)
            && message.Dispatch(messageTypes, TestExecutionSummaryEvent);
    }
}
