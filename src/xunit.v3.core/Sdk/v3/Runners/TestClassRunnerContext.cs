using System.Collections.Generic;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base context class for <see cref="TestClassRunner{TContext, TTestCase}"/>.
/// </summary>
public class TestClassRunnerContext<TTestCase> : ContextBase
	where TTestCase : _ITestCase
{
	ITestCaseOrderer testCaseOrderer;

	/// <summary>
	/// Initializes a new instancew of the <see cref="TestClassRunnerContext{TTestCase}"/> class.
	/// </summary>
	public TestClassRunnerContext(
		_ITestClass testClass,
		_IReflectionTypeInfo @class,
		IReadOnlyCollection<TTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource) :
			base(explicitOption, messageBus, aggregator, cancellationTokenSource)
	{
		TestClass = Guard.ArgumentNotNull(testClass);
		Class = Guard.ArgumentNotNull(@class);
		TestCases = Guard.ArgumentNotNull(testCases);
		this.testCaseOrderer = Guard.ArgumentNotNull(testCaseOrderer);
	}

	/// <summary>
	/// Gets the type information for this test class.
	/// </summary>
	public _IReflectionTypeInfo Class { get; }

	/// <summary>
	/// Gets or sets the orderer used to order the test cases.
	/// </summary>
	public ITestCaseOrderer TestCaseOrderer
	{
		get => testCaseOrderer;
		set => testCaseOrderer = Guard.ArgumentNotNull(value, nameof(TestCaseOrderer));
	}

	/// <summary>
	/// Gets the test cases associated with the test class.
	/// </summary>
	public IReadOnlyCollection<TTestCase> TestCases { get; }

	/// <summary>
	/// Gets the test class that is being executed.
	/// </summary>
	public _ITestClass TestClass { get; }
}
