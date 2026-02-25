using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test class runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public class CodeGenTestClassRunner : CoreTestClassRunner<CodeGenTestClassRunnerContext, ICodeGenTestClass, ICodeGenTestMethod, ICodeGenTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CodeGenTestClassRunner"/> class.
	/// </summary>
	protected CodeGenTestClassRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of <see cref="CodeGenTestClassRunner"/>.
	/// </summary>
	public static CodeGenTestClassRunner Instance { get; } = new();

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestClassFinished(
		CodeGenTestClassRunnerContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		await ctxt.Aggregator.RunAsync(ctxt.ClassFixtureMappings.DisposeAsync);
		return await base.OnTestClassFinished(ctxt, summary);
	}

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestClassStarting(CodeGenTestClassRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var result = await base.OnTestClassStarting(ctxt);
		await ctxt.Aggregator.RunAsync(() => ctxt.ClassFixtureMappings.InitializeAsync(
			createInstances: ctxt.TestCases.Any(tc => !tc.IsStaticallySkipped())
		));
		return result;
	}

	/// <summary>
	/// Runs the test class.
	/// </summary>
	/// <param name="testClass">The test class to be run.</param>
	/// <param name="testCases">The test cases to be run. Cannot be empty.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="collectionFixtureMappings">The mapping of collection fixture types to fixtures.</param>
	public async ValueTask<RunSummary> Run(
		ICodeGenTestClass testClass,
		IReadOnlyCollection<ICodeGenTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager collectionFixtureMappings)
	{
		Guard.ArgumentNotNull(testClass);
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(cancellationTokenSource);
		Guard.ArgumentNotNull(collectionFixtureMappings);

		await using var ctxt = new CodeGenTestClassRunnerContext(
			testClass,
			testCases,
			explicitOption,
			messageBus,
			aggregator,
			cancellationTokenSource,
			collectionFixtureMappings
		);
		await ctxt.InitializeAsync();

		return await ctxt.Aggregator.RunAsync(() => Run(ctxt), default);
	}

	/// <inheritdoc/>
	protected override void SetTestContext(
		CodeGenTestClassRunnerContext ctxt,
		TestEngineStatus testClassStatus)
	{
		Guard.ArgumentNotNull(ctxt);

		TestContext.SetForTestClass(ctxt.TestClass, testClassStatus, ctxt.CancellationTokenSource.Token, ctxt.ClassFixtureMappings);
	}
}
