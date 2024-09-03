using System;
using System.Reflection;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base context class for <see cref="TestInvoker{TContext, TTest}"/>.
/// </summary>
/// <typeparam name="TTest">The type of the test that the test framework uses. Must be derived
/// from <see cref="ITest"/>.</typeparam>
public class TestInvokerContext<TTest> : ContextBase
	where TTest : class, ITest
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestInvokerContext{TTest}"/> class.
	/// </summary>
	/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
	/// <param name="messageBus">The message bus to send execution messages to</param>
	/// <param name="aggregator">The exception aggregator</param>
	/// <param name="cancellationTokenSource">The cancellation token source</param>
	/// <param name="test">The test</param>
	/// <param name="testClass">The type of the test class</param>
	/// <param name="testClassInstance">The test class instance</param>
	/// <param name="testMethod">The test method</param>
	/// <param name="testMethodArguments">The method arguments for the test method</param>
	public TestInvokerContext(
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		TTest test,
		Type testClass,
		object? testClassInstance,
		MethodInfo testMethod,
		object?[] testMethodArguments) :
			base(explicitOption, messageBus, aggregator, cancellationTokenSource)
	{
		Test = Guard.ArgumentNotNull(test);
		TestClass = Guard.ArgumentNotNull(testClass);
		TestClassInstance = testClassInstance;
		TestMethod = Guard.ArgumentNotNull(testMethod);
		TestMethodArguments = Guard.ArgumentNotNull(testMethodArguments);

		// https://github.com/xunit/visualstudio.xunit/issues/371
		if (TestMethodArguments.Length == 0 && TestMethod.GetParameters().Length == 1)
		{
			var parameter = TestMethod.GetParameters()[0];
			var elementType = parameter.ParameterType.GetElementType();
			if (parameter.GetCustomAttribute<ParamArrayAttribute>() is not null && elementType is not null)
				TestMethodArguments = [Array.CreateInstance(elementType, 0)];
		}
	}

	/// <summary>
	/// Gets the test that's being invoked.
	/// </summary>
	public TTest Test { get; }

	/// <summary>
	/// Gets the type of the test class that this test originated in.
	/// </summary>
	public Type TestClass { get; }

	/// <summary>
	/// Gets the instance of the test class that's being invoked.
	/// </summary>
	public object? TestClassInstance { get; }

	/// <summary>
	/// Gets the method that this test originated in.
	/// </summary>
	public MethodInfo TestMethod { get; }

	/// <summary>
	/// Gets the arguments to be passed to the test method during invocation.
	/// </summary>
	public object?[] TestMethodArguments { get; }
}
