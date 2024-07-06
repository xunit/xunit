using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestClassRunner"/>.
/// </summary>
/// <param name="testClass">The test class</param>
/// <param name="testCases">The test from the test class</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="testCaseOrderer">The orderer used to sort the test cases for executiong</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="collectionFixtureMappings">The fixtures attached to the test collection</param>
public class XunitTestClassRunnerContext(
	IXunitTestClass testClass,
	IReadOnlyCollection<IXunitTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ITestCaseOrderer testCaseOrderer,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	FixtureMappingManager collectionFixtureMappings) :
		TestClassRunnerContext<IXunitTestClass, IXunitTestCase>(testClass, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource)
{
	ITestCaseOrderer testCaseOrderer = Guard.ArgumentNotNull(testCaseOrderer);

	/// <summary>
	/// Gets the mapping manager for class-level fixtures.
	/// </summary>
	public FixtureMappingManager ClassFixtureMappings { get; } = new("Class", Guard.ArgumentNotNull(collectionFixtureMappings));

	/// <summary>
	/// Gets or sets the orderer used to order the test cases.
	/// </summary>
	public ITestCaseOrderer TestCaseOrderer
	{
		get => testCaseOrderer;
		set => testCaseOrderer = Guard.ArgumentNotNull(value, nameof(TestCaseOrderer));
	}

	/// <inheritdoc/>
	public override async ValueTask InitializeAsync()
	{
		await base.InitializeAsync();

		var testCaseOrderer = TestClass.TestCaseOrderer;
		if (testCaseOrderer is not null)
			TestCaseOrderer = testCaseOrderer;
	}
}
