using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a single test case from xUnit.net v3 based on reflection.
/// </summary>
public interface IXunitTestCase : _ITestCase
{
	/// <summary>
	/// Gets a flag indicating whether this test case was marked as explicit or not.
	/// </summary>
	bool Explicit { get; }

	/// <summary>
	/// Gets the test class that this test case belongs to.
	/// </summary>
	new IXunitTestClass TestClass { get; }

	/// <summary>
	/// Gets the name of the class where the test is defined.
	/// </summary>
	new string TestClassName { get; }

	/// <summary>
	/// Gets the test collection this test case belongs to.
	/// </summary>
	new IXunitTestCollection TestCollection { get; }

	/// <summary>
	/// Gets the test method this test case belongs to.
	/// </summary>
	new IXunitTestMethod TestMethod { get; }

	/// <summary>
	/// Gets the method name where the test is defined.
	/// </summary>
	new string TestMethodName { get; }

	/// <summary>
	/// Gets the timeout of the test, in milliseconds; if zero or negative, means the test case has no timeout.
	/// </summary>
	int Timeout { get; }

	/// <summary>
	/// Executes the test case, returning 0 or more result messages through the message sink.
	/// </summary>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report results to.</param>
	/// <param name="constructorArguments">The arguments to pass to the constructor.</param>
	/// <param name="aggregator">The error aggregator to use for catching exception.</param>
	/// <param name="cancellationTokenSource">The cancellation token source that indicates whether cancellation has been requested.</param>
	/// <returns>Returns the summary of the test case run.</returns>
	ValueTask<RunSummary> RunAsync(
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		object?[] constructorArguments,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource
	);
}
