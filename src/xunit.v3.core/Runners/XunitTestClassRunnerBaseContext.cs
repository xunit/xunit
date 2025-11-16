using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestClassRunnerBase{TContext, TTestClass, TTestMethod, TTestCase}"/>.
/// </summary>
/// <param name="testClass">The test class</param>
/// <param name="testCases">The test from the test class</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="testMethodOrderer">The orderer used to sort the test methods in the class</param>
/// <param name="testCaseOrderer">The orderer used to sort the test cases in the class</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="collectionFixtureMappings">The fixtures attached to the test collection</param>
public class XunitTestClassRunnerBaseContext<TTestClass, TTestCase>(
	TTestClass testClass,
	IReadOnlyCollection<TTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ITestMethodOrderer testMethodOrderer,
	ITestCaseOrderer testCaseOrderer,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	FixtureMappingManager collectionFixtureMappings) :
		TestClassRunnerContext<TTestClass, TTestCase>(testClass, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource)
			where TTestClass : class, IXunitTestClass
			where TTestCase : class, IXunitTestCase
{
	ITestCaseOrderer testCaseOrderer = Guard.ArgumentNotNull(testCaseOrderer);
	ITestMethodOrderer testMethodOrderer = Guard.ArgumentNotNull(testMethodOrderer);

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

	/// <summary>
	/// Gets or sets the orderer used to order the test cases.
	/// </summary>
	public ITestMethodOrderer TestMethodOrderer
	{
		get => testMethodOrderer;
		set => testMethodOrderer = Guard.ArgumentNotNull(value, nameof(TestMethodOrderer));
	}

	/// <inheritdoc/>
	public override async ValueTask InitializeAsync()
	{
		await base.InitializeAsync();

		TestCaseOrderer =
			TestClass.TestCaseOrderer
				?? TestClass.TestCollection.TestCaseOrderer
				?? TestClass.TestCollection.TestAssembly.TestCaseOrderer
				?? TestCaseOrderer;
		TestMethodOrderer =
			TestClass.TestMethodOrderer
				?? TestClass.TestCollection.TestMethodOrderer
				?? TestClass.TestCollection.TestAssembly.TestMethodOrderer
				?? TestMethodOrderer;
	}
}
