using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test collection runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public class CodeGenTestCollectionRunner :
	CoreTestCollectionRunner<CodeGenTestCollectionRunnerContext, ICodeGenTestCollection, ICodeGenTestClass, ICodeGenTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CodeGenTestCollectionRunner"/> class.
	/// </summary>
	protected CodeGenTestCollectionRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of <see cref="CodeGenTestCollectionRunner"/>.
	/// </summary>
	public static CodeGenTestCollectionRunner Instance { get; } = new();

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestCollectionFinished(
		CodeGenTestCollectionRunnerContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		await ctxt.Aggregator.RunAsync(ctxt.CollectionFixtureMappings.DisposeAsync);
		return await base.OnTestCollectionFinished(ctxt, summary);
	}

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestCollectionStarting(CodeGenTestCollectionRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var result = await base.OnTestCollectionStarting(ctxt);
		await ctxt.Aggregator.RunAsync(() => ctxt.CollectionFixtureMappings.InitializeAsync(
			createInstances: ctxt.TestCases.Any(tc => !tc.IsStaticallySkipped())
		));
		return result;
	}

	/// <summary>
	/// Runs the test collection.
	/// </summary>
	/// <param name="testCollection">The test collection to be run.</param>
	/// <param name="testCases">The test cases to be run. Cannot be empty.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collection exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="assemblyFixtureMappings">The mapping manager for assembly fixtures.</param>
	public async ValueTask<RunSummary> Run(
		ICodeGenTestCollection testCollection,
		IReadOnlyCollection<ICodeGenTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager assemblyFixtureMappings)
	{
		Guard.ArgumentNotNull(testCollection);
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(cancellationTokenSource);
		Guard.ArgumentNotNull(assemblyFixtureMappings);

		await using var ctxt = new CodeGenTestCollectionRunnerContext(
			testCollection,
			testCases,
			explicitOption,
			messageBus,
			aggregator,
			cancellationTokenSource,
			assemblyFixtureMappings
		);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}
}
