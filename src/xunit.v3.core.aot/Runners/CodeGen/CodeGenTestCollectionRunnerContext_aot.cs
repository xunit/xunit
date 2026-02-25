using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="CodeGenTestAssemblyRunner"/>.
/// </summary>
/// <param name="testCollection">The test collection to be run.</param>
/// <param name="testCases">The test cases to be run. Cannot be empty.</param>
/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
/// <param name="messageBus">The message bus to report run status to.</param>
/// <param name="aggregator">The exception aggregator used to run code and collection exceptions.</param>
/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
/// <param name="assemblyFixtureMappings">The mapping manager for assembly fixtures.</param>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public class CodeGenTestCollectionRunnerContext(
	ICodeGenTestCollection testCollection,
	IReadOnlyCollection<ICodeGenTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	FixtureMappingManager assemblyFixtureMappings) :
		CoreTestCollectionRunnerContext<ICodeGenTestCollection, ICodeGenTestClass, ICodeGenTestCase>(
			testCollection,
			testCases,
			explicitOption,
			messageBus,
			aggregator,
			cancellationTokenSource
		)
{
	/// <summary>
	/// Gets the mapping manager for assembly-level fixtures.
	/// </summary>
	public FixtureMappingManager CollectionFixtureMappings { get; } = new("Collection", Guard.ArgumentNotNull(testCollection).CollectionFixtureFactories, assemblyFixtureMappings);

	/// <inheritdoc/>
	public override async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		await CollectionFixtureMappings.SafeDisposeAsync();
		await base.DisposeAsync();
	}

	/// <inheritdoc/>
	public override ValueTask<RunSummary> RunTestClass(
		ICodeGenTestClass testClass,
		IReadOnlyCollection<ICodeGenTestCase> testCases) =>
			CodeGenTestClassRunner.Instance.Run(
				testClass,
				testCases,
				ExplicitOption,
				MessageBus,
				Aggregator.Clone(),
				CancellationTokenSource,
				CollectionFixtureMappings
			);
}
