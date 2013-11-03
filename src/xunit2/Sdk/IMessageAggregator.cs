using System.Collections.Generic;

namespace Xunit.Sdk
{
    /// <summary>
    /// Represents an aggregator which collects and returns messages of arbitrary types.
    /// </summary>
    public interface IMessageAggregator
    {
        /// <summary>
        /// Adds the specified message to the aggregation for the given type.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="message">The message.</param>
        void Add<TMessage>(TMessage message);

        /// <summary>
        /// Returns all the currently aggregated messages of the given type, and clears
        /// the list for future iterations.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <returns>The aggregated messages.</returns>
        IEnumerable<TMessage> GetAndClear<TMessage>();
    }
}