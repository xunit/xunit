using System;
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
		IReadOnlyDictionary<Type, object> assemblyFixtureMappings) :
			base(testCollection, testCases, explicitOption, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
	{
		AssemblyFixtureMappings = assemblyFixtureMappings;
	}

	/// <summary>
	/// Gets the fixtures (mapped type => instance) that were declared at the assembly level.
	/// </summary>
	public IReadOnlyDictionary<Type, object> AssemblyFixtureMappings { get; }

	/// <summary>
	/// Gets the fixtures (mapped type => instance) that were declared at the collection level.
	/// </summary>
	public Dictionary<Type, object> CollectionFixtureMappings { get; } = new();
}
