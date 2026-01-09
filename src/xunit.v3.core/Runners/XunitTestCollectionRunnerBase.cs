using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

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

	/// <summary>
	/// Orders the test classes using the test class orderer from
	/// <see cref="XunitTestCollectionRunnerBaseContext{TTestCollection, TTestCase}.TestClassOrderer"/>.
	/// If the user hasn't provided an override, the default orderer is <see cref="DefaultTestClassOrderer"/>,
	/// which orders them in an unpredictable but stable order based on their unique ID.
	/// </summary>
	/// <inheritdoc/>
	protected override List<(TTestClass? Class, List<TTestCase> TestCases)> OrderTestClasses(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var testCasesByClass =
			ctxt.TestCases
				.GroupBy(tc => tc.TestClass as TTestClass, TestClassComparer<TTestClass>.Instance)
				.ToDictionary(group => new Maybe<TTestClass>(group.Key), group => group.ToList());

		try
		{
			var orderedTestClasses = ctxt.TestClassOrderer.OrderTestClasses(testCasesByClass.Keys.Select(k => k.Value).CastOrToReadOnlyCollection());

			return
				orderedTestClasses
					.Select(testClass => (testClass, testCasesByClass[testClass]))
					.ToList();
		}
		catch (Exception ex)
		{
			var innerEx = ex.Unwrap();

			ctxt.MessageBus.QueueMessage(new ErrorMessage()
			{
				AssemblyUniqueID = ctxt.TestCollection.TestAssembly.UniqueID,
				ExceptionParentIndices = [-1],
				ExceptionTypes = [typeof(TestPipelineException).SafeName()],
				Messages = [
					string.Format(
						CultureInfo.CurrentCulture,
						"Test class orderer '{0}' threw '{1}' during ordering: {2}",
						ctxt.TestClassOrderer.GetType().SafeName(),
						innerEx.GetType().SafeName(),
						innerEx.Message
					)
				],
				StackTraces = [innerEx.StackTrace],
			});

			return [];
		}
	}
}
