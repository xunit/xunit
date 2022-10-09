using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base context class for <see cref="TestCaseRunner{TContext, TTestCase}"/>.
/// </summary>
public class TestCaseRunnerContext<TTestCase> : ContextBase
	where TTestCase : _ITestCase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestCaseRunnerContext{TTestCase}"/> class.
	/// </summary>
	public TestCaseRunnerContext(
		TTestCase testCase,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource) :
			base(explicitOption, messageBus, aggregator, cancellationTokenSource)
	{
		TestCase = Guard.GenericArgumentNotNull(testCase);
	}

	/// <summary>
	/// Gets the test case that is being executed.
	/// </summary>
	public TTestCase TestCase { get; }
}
