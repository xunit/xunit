using System;
using System.Reflection;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base context class for <see cref="TestRunner{TContext}"/>.
/// </summary>
public class TestRunnerContext
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestRunnerContext"/> class.
	/// </summary>
	public TestRunnerContext(
		_ITest test,
		IMessageBus messageBus,
		Type testClass,
		object?[] constructorArguments,
		MethodInfo testMethod,
		object?[]? testMethodArguments,
		string? skipReason,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource)
	{
		Test = Guard.ArgumentNotNull(test);
		TestClass = Guard.ArgumentNotNull(testClass);
		ConstructorArguments = Guard.ArgumentNotNull(constructorArguments);
		TestMethod = Guard.ArgumentNotNull(testMethod);
		TestMethodArguments = testMethodArguments;
		MessageBus = Guard.ArgumentNotNull(messageBus);
		SkipReason = skipReason;
		Aggregator = aggregator;
		CancellationTokenSource = Guard.ArgumentNotNull(cancellationTokenSource);
	}

	/// <summary>
	/// Gets the aggregator used for reporting exceptions.
	/// </summary>
	public ExceptionAggregator Aggregator { get; }

	/// <summary>
	/// Gets the cancellation token source used for cancelling test execution.
	/// </summary>
	public CancellationTokenSource CancellationTokenSource { get; }

	/// <summary>
	/// Gets the arguments that should be passed to the test class when it's constructed.
	/// </summary>
	public object?[] ConstructorArguments { get; }

	/// <summary>
	/// Gets the message bus to send execution engine messages to.
	/// </summary>
	public IMessageBus MessageBus { get; }

	/// <summary>
	/// Gets the skip reason given for the test; will be <c>null</c> if the test is not
	/// statically skipped. (It may still be dynamically skipped at a later time.)
	/// </summary>
	public string? SkipReason { get; }

	/// <summary>
	/// Gets the test that's being invoked.
	/// </summary>
	public _ITest Test { get; }

	/// <summary>
	/// Gets the type of the test class that this test originated in.
	/// </summary>
	public Type TestClass { get; }

	/// <summary>
	/// Gets the method that this test originated in.
	/// </summary>
	public MethodInfo TestMethod { get; }

	/// <summary>
	/// Gets the arguments to be passed to the test method during invocation.
	/// </summary>
	public object?[]? TestMethodArguments { get; }
}
