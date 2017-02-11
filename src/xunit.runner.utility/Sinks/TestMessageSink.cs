using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IMessageSinkWithTypes"/> that provides access to events for all
    /// levels of reporting.
    /// </summary>
    public class TestMessageSink : AggregateMessageSink, IMessageSink
    {
        DiagnosticEventSink diagnostics;
        DiscoveryEventSink discovery;
        ExecutionEventSink execution;
        RunnerEventSink runner;

        /// <summary>
        /// Gets a list of diagnostics events that can be subscribed to.
        /// </summary>
        public DiagnosticEventSink Diagnostics => GetOrCreateAggregatedSink(ref diagnostics);

        /// <summary>
        /// Gets a list of discovery events that can be subscribed to.
        /// </summary>
        public DiscoveryEventSink Discovery => GetOrCreateAggregatedSink(ref discovery);

        /// <summary>
        /// Gets a list of execution events that can be subscribed to.
        /// </summary>
        public ExecutionEventSink Execution => GetOrCreateAggregatedSink(ref execution);

        /// <summary>
        /// Gets a list of runner events that can be subscribed to.
        /// </summary>
        public RunnerEventSink Runner => GetOrCreateAggregatedSink(ref runner);

        /// <inheritdoc/>
        public bool OnMessage(IMessageSinkMessage message)
            => OnMessageWithTypes(message, MessageSinkAdapter.GetImplementedInterfaces(message));
    }
}
