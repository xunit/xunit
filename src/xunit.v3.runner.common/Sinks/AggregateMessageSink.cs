using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IMessageSink"/> which dispatches messages
/// to one or more individual message sinks.
/// </summary>
public class AggregateMessageSink : IMessageSink, IAsyncDisposable
{
	DisposalTracker disposalTracker = new();

	/// <summary>
	/// The list of event dispatchers that are registered with the system.
	/// </summary>
	protected List<IMessageSink> AggregatedSinks { get; } = [];

	/// <inheritdoc/>
	public virtual ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		var tracker = default(DisposalTracker);

		lock (disposalTracker)
		{
			tracker = disposalTracker;
			disposalTracker = new DisposalTracker();
			AggregatedSinks.Clear();
		}

		return tracker.SafeDisposeAsync();
	}

	/// <summary>
	/// Gets a dispatcher, optionally creating and registering it if it doesn't exist.
	/// </summary>
	/// <typeparam name="TDispatcher">The type of the dispatcher</typeparam>
	/// <param name="value">The dispatcher</param>
	/// <returns>The dispatcher</returns>
	protected TDispatcher GetOrCreateAggregatedSink<TDispatcher>(ref TDispatcher? value)
		where TDispatcher : class, IMessageSink, new()
	{
		if (value is null)
			lock (AggregatedSinks)
			{
				value = new TDispatcher();
				AggregatedSinks.Add(value);
			}

		return value;
	}

	/// <inheritdoc/>
	public virtual bool OnMessage(IMessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		var result = true;

		lock (AggregatedSinks)
			foreach (var dispatcher in AggregatedSinks)
				result = dispatcher.OnMessage(message) && result;

		return result;
	}
}
