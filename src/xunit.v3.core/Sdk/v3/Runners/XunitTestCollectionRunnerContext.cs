using System.Collections.Generic;
using System.Threading;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestCollectionRunner"/>.
/// </summary>
public class XunitTestCollectionRunnerContext : TestCollectionRunnerContext<IXunitTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestCollectionRunnerContext"/> class.
	/// </summary>
	public XunitTestCollectionRunnerContext(
		_ITestCollection testCollection,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager assemblyFixtureMappings) :
			base(testCollection, testCases, explicitOption, messageBus, testCaseOrderer, aggregator, cancellationTokenSource) =>
				CollectionFixtureMappings = new("Collection", assemblyFixtureMappings);

	/// <summary>
	/// Gets the mapping manager for collection-level fixtures.
	/// </summary>
	public FixtureMappingManager CollectionFixtureMappings { get; }
}
