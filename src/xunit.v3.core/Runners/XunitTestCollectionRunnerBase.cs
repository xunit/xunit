using System.Linq;
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
		await ctxt.Aggregator.RunAsync(() => ctxt.CollectionFixtureMappings.InitializeAsync(
			ctxt.TestCollection.CollectionFixtureTypes,
			createInstances: ctxt.TestCases.Any(tc => !tc.IsStaticallySkipped())
		));
		return result;
	}
}
