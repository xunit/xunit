using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Test assembly runner for xUnit.net v3 tests (with overridable context).
/// </summary>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestAssemblyRunnerBase<TContext, TTestAssembly, TTestCollection, TTestCase> :
	CoreTestAssemblyRunner<TContext, TTestAssembly, TTestCollection, TTestCase>
		where TContext : XunitTestAssemblyRunnerBaseContext<TTestAssembly, TTestCollection, TTestCase>
		where TTestAssembly : class, IXunitTestAssembly
		where TTestCollection : class, IXunitTestCollection
		where TTestCase : class, IXunitTestCase
{
	/// <inheritdoc/>
	protected override ValueTask<string> GetTestFrameworkDisplayName(TContext ctxt) =>
		new(XunitTestFramework.DisplayName);

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestAssemblyFinished(
		TContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		await ctxt.Aggregator.RunAsync(ctxt.AssemblyFixtureMappings.DisposeAsync);
		return await base.OnTestAssemblyFinished(ctxt, summary);
	}

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestAssemblyStarting(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var result = await base.OnTestAssemblyStarting(ctxt);
		await ctxt.Aggregator.RunAsync(() => ctxt.AssemblyFixtureMappings.InitializeAsync(
			ctxt.TestAssembly.AssemblyFixtureTypes,
			createInstances: ctxt.TestCases.Any(tc => !tc.IsStaticallySkipped())
		));
		return result;
	}
}
