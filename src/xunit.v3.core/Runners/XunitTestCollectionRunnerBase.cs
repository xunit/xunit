using System.Threading.Tasks;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// The test collection runner for xUnit.net v3 tests (with overridable context).
/// </summary>
public abstract class XunitTestCollectionRunnerBase<TContext, TTestCollection, TTestClass, TTestCase> :
	TestCollectionRunner<TContext, TTestCollection, TTestClass, TTestCase>
		where TContext : XunitTestCollectionRunnerBaseContext<TTestCollection, TTestCase>
		where TTestCollection : class, IXunitTestCollection
		where TTestClass : class, IXunitTestClass
		where TTestCase : class, IXunitTestCase
{
	/// <summary>
	/// Gives an opportunity to override test case orderer. By default, this method gets the
	/// orderer from the collection definition. If this function returns <c>null</c>, the
	/// test case orderer passed into the constructor will be used.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test collection</param>
	protected virtual ITestCaseOrderer? GetTestCaseOrderer(TContext ctxt) =>
		Guard.ArgumentNotNull(ctxt).TestCollection.TestCaseOrderer;

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestCollectionFinished(
		TContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		await ctxt.Aggregator.RunAsync(ctxt.CollectionFixtureMappings.DisposeAsync);
		return await base.OnTestCollectionFinished(ctxt, summary);
	}

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestCollectionStarting(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var result = await base.OnTestCollectionStarting(ctxt);
		await ctxt.Aggregator.RunAsync(() => ctxt.CollectionFixtureMappings.InitializeAsync(ctxt.TestCollection.CollectionFixtureTypes));
		return result;
	}
}
