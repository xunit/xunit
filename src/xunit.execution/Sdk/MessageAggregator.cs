using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IMessageAggregator"/>.
    /// </summary>
    public class MessageAggregator : IMessageAggregator
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="MessageAggregator"/>.
        /// </summary>
        public readonly static IMessageAggregator Instance = new MessageAggregator();

        readonly ConcurrentDictionary<Type, object> observers = new ConcurrentDictionary<Type, object>();

        private MessageAggregator() { }

        /// <inheritdoc/>
        public void Add<TMessage>(TMessage message)
        {
            GetMessageBag<TMessage>().Add(message);
        }

        /// <inheritdoc/>
        public IEnumerable<TMessage> GetAndClear<TMessage>()
        {
            var bag = GetMessageBag<TMessage>();
            TMessage message;

            while (bag.TryTake(out message))
                yield return message;
        }

        ConcurrentBag<TMessage> GetMessageBag<TMessage>()
        {
            return (ConcurrentBag<TMessage>)observers.GetOrAdd(typeof(TMessage), t => new ConcurrentBag<TMessage>());
        }
    }
}
