using System.Collections.Generic;
using System.Threading;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestMethodRunner"/>.
/// </summary>
public class XunitTestMethodRunnerContext : TestMethodRunnerContext<IXunitTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestMethodRunnerContext"/> record.
	/// </summary>
	public XunitTestMethodRunnerContext(
		_ITestClass testClass,
		_ITestMethod testMethod,
		_IReflectionTypeInfo @class,
		_IReflectionMethodInfo method,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		object?[] constructorArguments) :
			base(testClass, testMethod, @class, method, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource)
	{
		ConstructorArguments = constructorArguments;
	}

	/// <summary>
	/// Gets the arguments to send to the test class constructor.
	/// </summary>
	public object?[] ConstructorArguments { get; }
}
