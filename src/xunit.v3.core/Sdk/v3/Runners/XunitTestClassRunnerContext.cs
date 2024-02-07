using System.Collections.Generic;
using System.Threading;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestClassRunner"/>.
/// </summary>
public class XunitTestClassRunnerContext : TestClassRunnerContext<IXunitTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestClassRunnerContext"/> record.
	/// </summary>
	public XunitTestClassRunnerContext(
		_ITestClass testClass,
		_IReflectionTypeInfo @class,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager collectionFixtureMappings) :
			base(testClass, @class, testCases, explicitOption, messageBus, testCaseOrderer, aggregator, cancellationTokenSource) =>
				ClassFixtureMappings = new("Class", collectionFixtureMappings);

	/// <summary>
	/// Gets the mapping manager for class-level fixtures.
	/// </summary>
	public FixtureMappingManager ClassFixtureMappings { get; }
}
