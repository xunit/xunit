using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base class for all execution pipeline context classes.
/// </summary>
public class ContextBase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ContextBase"/> class.
	/// </summary>
	/// <param name="messageBus"></param>
	/// <param name="aggregator"></param>
	/// <param name="cancellationTokenSource"></param>
	public ContextBase(
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource)
	{
		MessageBus = Guard.ArgumentNotNull(messageBus);
		Aggregator = aggregator;
		CancellationTokenSource = cancellationTokenSource;
	}

	/// <summary>
	/// Gets the aggregator used for reporting exceptions.
	/// </summary>
	public ExceptionAggregator Aggregator { get; }

	/// <summary>
	/// Gets the cancellation token source used for cancelling test execution.
	/// </summary>
	public CancellationTokenSource CancellationTokenSource { get; }

	/// <summary>
	/// Gets the message bus to send execution engine messages to.
	/// </summary>
	public IMessageBus MessageBus { get; }
}
