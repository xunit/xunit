using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IMessageSinkWithTypes"/> which dispatches messages
    /// to one or more individual message sinks.
    /// </summary>
    public class AggregateMessageSink : LongLivedMarshalByRefObject, IMessageSinkWithTypes
    {
        /// <summary>
        /// The list of event dispatchers that are registered with the system.
        /// </summary>
        protected List<IMessageSinkWithTypes> AggregatedSinks { get; } = new List<IMessageSinkWithTypes>();

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            lock (AggregatedSinks)
            {
                foreach (var sink in AggregatedSinks)
                    sink.SafeDispose();

                AggregatedSinks.Clear();
            }
        }

        /// <summary>
        /// Gets a dispatcher, optionally creating and registering it if it doesn't exist.
        /// </summary>
        /// <typeparam name="TDispatcher">The type of the dispatcher</typeparam>
        /// <param name="value">The dispatcher</param>
        /// <returns>The dispatcher</returns>
        protected TDispatcher GetOrCreateAggregatedSink<TDispatcher>(ref TDispatcher value)
            where TDispatcher : IMessageSinkWithTypes, new()
        {
            if (value == null)
            {
                lock (AggregatedSinks)
                {
                    if (value == null)
                    {
                        value = new TDispatcher();
                        AggregatedSinks.Add(value);
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Reports the presence of a message on the message bus with an optional list of message types.
        /// This method should never throw exceptions.
        /// </summary>
        /// <param name="message">The message from the message bus.</param>
        /// <param name="messageTypes">The list of message types, or <c>null</c>.</param>
        /// <returns>Return <c>true</c> to continue running tests, or <c>false</c> to stop.</returns>
        public virtual bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> messageTypes)
        {
            var result = true;

            lock (AggregatedSinks)
                foreach (var dispatcher in AggregatedSinks)
                    result = dispatcher.OnMessageWithTypes(message, messageTypes) && result;

            return result;
        }
    }
}
