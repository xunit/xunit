using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base class for all execution pipeline context classes.
/// </summary>
public class ContextBase : IAsyncLifetime
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ContextBase"/> class.
	/// </summary>
	public ContextBase(
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource)
	{
		MessageBus = Guard.ArgumentNotNull(messageBus);
		Aggregator = aggregator;
		CancellationTokenSource = Guard.ArgumentNotNull(cancellationTokenSource);
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

	/// <inheritdoc/>
	public virtual ValueTask DisposeAsync() =>
		default;

	/// <inheritdoc/>
	public virtual ValueTask InitializeAsync() =>
		default;
}
