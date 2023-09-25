using System;
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
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource)
	{
		ExplicitOption = explicitOption;
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
	/// Gets a flag which indicates how explicit tests should be handled.
	/// </summary>
	public ExplicitOption ExplicitOption { get; }

	/// <summary>
	/// Gets the message bus to send execution engine messages to.
	/// </summary>
	public IMessageBus MessageBus { get; }

	/// <inheritdoc/>
	public virtual ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		return default;
	}

	/// <inheritdoc/>
	public virtual ValueTask InitializeAsync() =>
		default;
}
