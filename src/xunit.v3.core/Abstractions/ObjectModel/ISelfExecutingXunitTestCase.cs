using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents an implementation of <see cref="IXunitTestCase"/> that is self-executing. This allows
/// the test case to opt into the middle of the test execution pipeline without implementing
/// everything that comes before it.
/// </summary>
public interface ISelfExecutingXunitTestCase : IXunitTestCase
{
	/// <summary>
	/// Executes the test case, returning 0 or more result messages through the message sink.
	/// </summary>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report results to.</param>
	/// <param name="constructorArguments">The arguments to pass to the constructor.</param>
	/// <param name="aggregator">The error aggregator to use for catching exception.</param>
	/// <param name="cancellationTokenSource">The cancellation token source that indicates whether cancellation has been requested.</param>
	/// <returns>Returns the summary of the test case run.</returns>
	ValueTask<RunSummary> Run(
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		object?[] constructorArguments,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource
	);
}
