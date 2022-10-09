using System.Collections.Generic;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base context class for <see cref="TestMethodRunner{TContext, TTestCase}"/>.
/// </summary>
public class TestMethodRunnerContext<TTestCase> : ContextBase
	where TTestCase : _ITestCase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestMethodRunnerContext{TTestCase}"/> class.
	/// </summary>
	public TestMethodRunnerContext(
		_ITestClass testClass,
		_ITestMethod testMethod,
		_IReflectionTypeInfo @class,
		_IReflectionMethodInfo method,
		IReadOnlyCollection<TTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource) :
			base(explicitOption, messageBus, aggregator, cancellationTokenSource)
	{
		TestClass = Guard.ArgumentNotNull(testClass);
		TestMethod = Guard.ArgumentNotNull(testMethod);
		Class = Guard.ArgumentNotNull(@class);
		Method = Guard.ArgumentNotNull(method);
		TestCases = Guard.ArgumentNotNull(testCases);
	}

	/// <summary>
	/// Gets the type information for this test class.
	/// </summary>
	public _IReflectionTypeInfo Class { get; }

	/// <summary>
	/// Gets the method information for this test method.
	/// </summary>
	public _IReflectionMethodInfo Method { get; }

	/// <summary>
	/// Gets the test cases that are derived from this test method.
	/// </summary>
	public IReadOnlyCollection<TTestCase> TestCases { get; }

	/// <summary>
	/// Gets the test class that this test method belongs to.
	/// </summary>
	public _ITestClass TestClass { get; }

	/// <summary>
	/// Gets the test method that is being executed.
	/// </summary>
	public _ITestMethod TestMethod { get; }
}
