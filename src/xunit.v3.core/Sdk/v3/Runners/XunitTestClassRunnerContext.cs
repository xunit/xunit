using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Internal;
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
		IReadOnlyDictionary<Type, object> assemblyFixtureMappings,
		IReadOnlyDictionary<Type, object> collectionFixtureMappings) :
			base(testClass, @class, testCases, explicitOption, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
	{
		AssemblyFixtureMappings = Guard.ArgumentNotNull(assemblyFixtureMappings);
		CollectionFixtureMappings = Guard.ArgumentNotNull(collectionFixtureMappings);
	}

	/// <summary>
	/// Gets the fixtures (mapped type => instance) that were declared at the assembly level.
	/// </summary>
	public IReadOnlyDictionary<Type, object> AssemblyFixtureMappings { get; }

	/// <summary>
	/// Gets the fixtures (mapped type => instance) that were declared at the class level.
	/// </summary>
	public Dictionary<Type, object> ClassFixtureMappings { get; } = new();

	/// <summary>
	/// Gets the fixtures (mapped type => instance) that were declared at the collection level.
	/// </summary>
	public IReadOnlyDictionary<Type, object> CollectionFixtureMappings { get; }

	/// <summary>
	/// Gets the list of class fixtures that were initialized via <see cref="IAsyncLifetime"/>.
	/// </summary>
	public HashSet<IAsyncLifetime> InitializedAsyncClassFixtures { get; } = new();
}
