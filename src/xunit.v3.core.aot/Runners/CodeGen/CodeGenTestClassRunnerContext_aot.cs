using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="CodeGenTestClassRunner"/>.
/// </summary>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public class CodeGenTestClassRunnerContext : CoreTestClassRunnerContext<ICodeGenTestClass, ICodeGenTestMethod, ICodeGenTestCase>
{
	/// <param name="testClass">The test class</param>
	/// <param name="testCases">The test from the test class</param>
	/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
	/// <param name="messageBus">The message bus to send execution messages to</param>
	/// <param name="aggregator">The exception aggregator</param>
	/// <param name="cancellationTokenSource">The cancellation token source</param>
	/// <param name="collectionFixtureMappings">The mapping of collection fixture types to fixtures.</param>
	public CodeGenTestClassRunnerContext(
		ICodeGenTestClass testClass,
		IReadOnlyCollection<ICodeGenTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager collectionFixtureMappings) :
			base(testClass, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource)
	{
		var classFixtureFactories = new Dictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>>(Guard.ArgumentNotNull(testClass).TestCollection.ClassFixtureFactories);
		foreach (var classLevelFactory in testClass.ClassFixtureFactories)
			classFixtureFactories[classLevelFactory.Key] = classLevelFactory.Value;

		ClassFixtureMappings = new("Class", classFixtureFactories, collectionFixtureMappings);
	}

	/// <summary>
	/// Gets the fixture mapping manager for the test class.
	/// </summary>
	public FixtureMappingManager ClassFixtureMappings { get; }

	/// <inheritdoc/>
	public override async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		await ClassFixtureMappings.SafeDisposeAsync();
		await base.DisposeAsync();
	}

	/// <inheritdoc/>
	public override ValueTask<RunSummary> RunTestMethod(
		ICodeGenTestMethod testMethod,
		IReadOnlyCollection<ICodeGenTestCase> testCases) =>
			CodeGenTestMethodRunner.Instance.Run(
				testMethod,
				testCases,
				ExplicitOption,
				MessageBus,
				Aggregator.Clone(),
				CancellationTokenSource,
				ClassFixtureMappings
			);
}
