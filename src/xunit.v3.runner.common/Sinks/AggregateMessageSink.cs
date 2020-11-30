using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// An implementation of <see cref="_IMessageSink"/> which dispatches messages
	/// to one or more individual message sinks.
	/// </summary>
	public class AggregateMessageSink : _IMessageSink, IAsyncDisposable
	{
		DisposalTracker disposalTracker = new DisposalTracker();

		/// <summary>
		/// The list of event dispatchers that are registered with the system.
		/// </summary>
		protected List<_IMessageSink> AggregatedSinks { get; } = new List<_IMessageSink>();

		/// <inheritdoc/>
		public virtual ValueTask DisposeAsync()
		{
			var tracker = default(DisposalTracker);

			lock (disposalTracker)
			{
				tracker = disposalTracker;
				disposalTracker = new DisposalTracker();
				AggregatedSinks.Clear();
			}

			return tracker.DisposeAsync();
		}

		/// <summary>
		/// Gets a dispatcher, optionally creating and registering it if it doesn't exist.
		/// </summary>
		/// <typeparam name="TDispatcher">The type of the dispatcher</typeparam>
		/// <param name="value">The dispatcher</param>
		/// <returns>The dispatcher</returns>
		protected TDispatcher GetOrCreateAggregatedSink<TDispatcher>(ref TDispatcher? value)
			where TDispatcher : class, _IMessageSink, new()
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

		/// <inheritdoc/>
		public virtual bool OnMessage(_MessageSinkMessage message)
		{
			Guard.ArgumentNotNull(nameof(message), message);

			var result = true;

			lock (AggregatedSinks)
				foreach (var dispatcher in AggregatedSinks)
					result = dispatcher.OnMessage(message) && result;

			return result;
		}
	}
}
