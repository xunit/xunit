using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Represents a single test case from xUnit.net v2.
    /// </summary>
    public interface IXunitTestCase : ITestCase
    {
        /// <inheritdoc/>
        new ISourceInformation SourceInformation { get; set; }

        /// <summary>
        /// Executes the test case, returning 0 or more result messages through the message sink.
        /// </summary>
        /// <param name="messageBus">The message bus to report results to.</param>
        /// <param name="constructorArguments">The arguments to pass to the constructor.</param>
        /// <param name="aggregator">The error aggregator to use for catching exception.</param>
        /// <param name="cancellationTokenSource">The cancellation token source that indicates whether cancellation has been requested.</param>
        Task RunAsync(IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource);
    }
}
