using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="CodeGenTestClassRunnerBase{TContext, TTestClass, TTestMethod, TTestCase}"/>.
/// </summary>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public abstract class CodeGenTestClassRunnerBaseContext<TTestClass, TTestMethod, TTestCase> : CoreTestClassRunnerContext<TTestClass, TTestMethod, TTestCase>
	where TTestClass : class, ICodeGenTestClass
	where TTestMethod : class, ICodeGenTestMethod
	where TTestCase : class, ICodeGenTestCase
{
	/// <param name="testClass">The test class</param>
	/// <param name="testCases">The test from the test class</param>
	/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
	/// <param name="messageBus">The message bus to send execution messages to</param>
	/// <param name="aggregator">The exception aggregator</param>
	/// <param name="parallelismOptions">Options which determine the amount of test parallelization to allow.</param>
	/// <param name="cancellationTokenSource">The cancellation token source</param>
	/// <param name="collectionFixtureMappings">The mapping of collection fixture types to fixtures.</param>
	protected CodeGenTestClassRunnerBaseContext(
		TTestClass testClass,
		IReadOnlyCollection<TTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		ParallelismOptions parallelismOptions,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager collectionFixtureMappings) :
			base(testClass, testCases, explicitOption, messageBus, aggregator, parallelismOptions, cancellationTokenSource)
	{
		var classFixtureFactories = new Dictionary<Type, FixtureFactory>(Guard.ArgumentNotNull(testClass).TestCollection.ClassFixtureFactories);
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
}
