using System;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit
{
    public partial class Assert
    {
        /// <summary>
        /// Verifies that a event with the exact event args is raised.
        /// </summary>
        /// <typeparam name="T">The type of the event arguments to expect</typeparam>
        /// <param name="attach">Code to attach the event handler</param>
        /// <param name="detach">Code to detach the event handler</param>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The event sender and arguments wrapped in an object</returns>
        /// <exception cref="RaisesException">Thrown when the expected event was not raised.</exception>
        public static RaisedEvent<T> Raises<T>(Action<EventHandler<T>> attach, Action<EventHandler<T>> detach, Action testCode)
            where T : EventArgs
        {
            var raisedEvent = RaisesInternal(attach, detach, testCode);

            if (raisedEvent == null)
                throw new RaisesException(typeof(T));

            if (!raisedEvent.Arguments.GetType().Equals(typeof(T)))
                throw new RaisesException(typeof(T), raisedEvent.Arguments.GetType());

            return raisedEvent;
        }

        /// <summary>
        /// Verifies that an event with the exact or a derived event args is raised.
        /// </summary>
        /// <typeparam name="T">The type of the event arguments to expect</typeparam>
        /// <param name="attach">Code to attach the event handler</param>
        /// <param name="detach">Code to detach the event handler</param>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The event sender and arguments wrapped in an object</returns>
        /// <exception cref="RaisesException">Thrown when the expected event was not raised.</exception>
        public static RaisedEvent<T> RaisesAny<T>(Action<EventHandler<T>> attach, Action<EventHandler<T>> detach, Action testCode)
            where T : EventArgs
        {
            var raisedEvent = RaisesInternal(attach, detach, testCode);

            if (raisedEvent == null)
                throw new RaisesException(typeof(T));

            return raisedEvent;
        }

        /// <summary>
        /// Verifies that a event with the exact event args (and not a derived type) is raised.
        /// </summary>
        /// <typeparam name="T">The type of the event arguments to expect</typeparam>
        /// <param name="attach">Code to attach the event handler</param>
        /// <param name="detach">Code to detach the event handler</param>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The event sender and arguments wrapped in an object</returns>
        /// <exception cref="RaisesException">Thrown when the expected event was not raised.</exception>
        public static async Task<RaisedEvent<T>> RaisesAsync<T>(Action<EventHandler<T>> attach, Action<EventHandler<T>> detach, Func<Task> testCode)
            where T : EventArgs
        {
            var raisedEvent = await RaisesAsyncInternal(attach, detach, testCode);

            if (raisedEvent == null)
                throw new RaisesException(typeof(T));

            if (!raisedEvent.Arguments.GetType().Equals(typeof(T)))
                throw new RaisesException(typeof(T), raisedEvent.Arguments.GetType());

            return raisedEvent;
        }

        /// <summary>
        /// Verifies that an event with the exact or a derived event args is raised.
        /// </summary>
        /// <typeparam name="T">The type of the event arguments to expect</typeparam>
        /// <param name="attach">Code to attach the event handler</param>
        /// <param name="detach">Code to detach the event handler</param>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The event sender and arguments wrapped in an object</returns>
        /// <exception cref="RaisesException">Thrown when the expected event was not raised.</exception>
        public static async Task<RaisedEvent<T>> RaisesAnyAsync<T>(Action<EventHandler<T>> attach, Action<EventHandler<T>> detach, Func<Task> testCode)
            where T : EventArgs
        {
            var raisedEvent = await RaisesAsyncInternal(attach, detach, testCode);

            if (raisedEvent == null)
                throw new RaisesException(typeof(T));

            return raisedEvent;
        }

        static RaisedEvent<T> RaisesInternal<T>(Action<EventHandler<T>> attach, Action<EventHandler<T>> detach, Action testCode)
            where T : EventArgs
        {
            RaisedEvent<T> raisedEvent = null;
            EventHandler<T> handler = delegate (object s, T args) { raisedEvent = new RaisedEvent<T>(s, args); };
            attach(handler);
            testCode();
            detach(handler);
            return raisedEvent;
        }

        static async Task<RaisedEvent<T>> RaisesAsyncInternal<T>(Action<EventHandler<T>> attach, Action<EventHandler<T>> detach, Func<Task> testCode)
            where T : EventArgs
        {
            RaisedEvent<T> raisedEvent = null;
            EventHandler<T> handler = delegate (object s, T args) { raisedEvent = new RaisedEvent<T>(s, args); };
            attach(handler);
            await testCode();
            detach(handler);
            return raisedEvent;
        }

        /// <summary>
        /// Represents a raised event after the fact.
        /// </summary>
        /// <typeparam name="T">The type of the event arguments.</typeparam>
        public class RaisedEvent<T>
            where T : EventArgs
        {
            /// <summary>
            /// The sender of the event.
            /// </summary>
            public object Sender { get; }

            /// <summary>
            /// The event arguments.
            /// </summary>
            public T Arguments { get; }

            /// <summary>
            /// Creates a new instance of the <see cref="RaisedEvent{T}" /> class.
            /// </summary>
            /// <param name="sender">The sender of the event.</param>
            /// <param name="args">The event arguments</param>
            public RaisedEvent(object sender, T args)
            {
                Sender = sender;
                Arguments = args;
            }
        }
    }
}
