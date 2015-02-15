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
        /// <summary>
        /// Gets the method to be run. Differs from <see cref="ITestCase.TestMethod"/>.<see cref="ITestMethod.Method"/> in that
        /// any generic argument types will have been closed based on the arguments.
        /// </summary>
        IMethodInfo Method { get; }

        /// <summary>
        /// Executes the test case, returning 0 or more result messages through the message sink.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages to.</param>
        /// <param name="messageBus">The message bus to report results to.</param>
        /// <param name="constructorArguments">The arguments to pass to the constructor.</param>
        /// <param name="aggregator">The error aggregator to use for catching exception.</param>
        /// <param name="cancellationTokenSource">The cancellation token source that indicates whether cancellation has been requested.</param>
        /// <returns>Returns the summary of the test case run.</returns>
        Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource);
    }
}
