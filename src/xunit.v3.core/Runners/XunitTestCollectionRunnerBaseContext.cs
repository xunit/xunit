using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestCollectionRunnerBaseContext{TTestCollection, TTestCase}"/>.
/// </summary>
/// <param name="testCollection">The test collection</param>
/// <param name="testCases">The test cases from the test collection</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="testClassOrderer">The order used to order tests classes in the collection</param>
/// <param name="testMethodOrderer">The order used to order tests methods in the collection</param>
/// <param name="testCaseOrderer">The order used to order tests cases in the collection</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="assemblyFixtureMappings">The fixtures associated with the test assembly</param>
public class XunitTestCollectionRunnerBaseContext<TTestCollection, TTestCase>(
	TTestCollection testCollection,
	IReadOnlyCollection<TTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ITestClassOrderer testClassOrderer,
	ITestMethodOrderer testMethodOrderer,
	ITestCaseOrderer testCaseOrderer,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	FixtureMappingManager assemblyFixtureMappings) :
		TestCollectionRunnerContext<TTestCollection, TTestCase>(testCollection, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource)
			where TTestCollection : class, IXunitTestCollection
			where TTestCase : class, IXunitTestCase
{
	ITestCaseOrderer testCaseOrderer = Guard.ArgumentNotNull(testCaseOrderer);
	ITestClassOrderer testClassOrderer = Guard.ArgumentNotNull(testClassOrderer);
	ITestMethodOrderer testMethodOrderer = Guard.ArgumentNotNull(testMethodOrderer);

	/// <summary>
	/// Please use <see cref="XunitTestCollectionRunnerBaseContext(TTestCollection, IReadOnlyCollection{TTestCase}, ExplicitOption, IMessageBus, ITestClassOrderer, ITestMethodOrderer, ITestCaseOrderer, ExceptionAggregator, CancellationTokenSource, FixtureMappingManager)"/>.
	/// This overload will be removed in the next major version.
	/// </summary>
	[Obsolete("Please use the constructor which accepts testClassOrderer and testMethodOrderer. This overload will be removed in the next major version.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[OverloadResolutionPriority(-1)]
	public XunitTestCollectionRunnerBaseContext(
		TTestCollection testCollection,
		IReadOnlyCollection<TTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager assemblyFixtureMappings) :
			this(
				testCollection,
				testCases,
				explicitOption,
				messageBus,
				DefaultTestClassOrderer.Instance,
				DefaultTestMethodOrderer.Instance,
				testCaseOrderer,
				aggregator,
				cancellationTokenSource,
				assemblyFixtureMappings
			)
	{ }

	/// <summary>
	/// Gets the mapping manager for collection-level fixtures.
	/// </summary>
	public FixtureMappingManager CollectionFixtureMappings { get; } = new("Collection", Guard.ArgumentNotNull(assemblyFixtureMappings));

	/// <summary>
	/// Gets or sets the orderer used to order test cases within the test collection.
	/// </summary>
	public ITestCaseOrderer TestCaseOrderer
	{
		get => testCaseOrderer;
		set => testCaseOrderer = Guard.ArgumentNotNull(value, nameof(TestCaseOrderer));
	}

	/// <summary>
	/// Gets or sets the orderer used to order test classes within the test collection.
	/// </summary>
	public ITestClassOrderer TestClassOrderer
	{
		get => testClassOrderer;
		set => testClassOrderer = Guard.ArgumentNotNull(value, nameof(TestClassOrderer));
	}

	/// <summary>
	/// Gets or sets the orderer used to order test methods within the test collection.
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
			TestCollection.TestCaseOrderer
				?? TestCollection.TestAssembly.TestCaseOrderer
				?? TestCaseOrderer;
		TestClassOrderer =
			TestCollection.TestClassOrderer
				?? TestCollection.TestAssembly.TestClassOrderer
				?? TestClassOrderer;
		TestMethodOrderer =
			TestCollection.TestMethodOrderer
				?? TestCollection.TestAssembly.TestMethodOrderer
				?? TestMethodOrderer;
	}
}
