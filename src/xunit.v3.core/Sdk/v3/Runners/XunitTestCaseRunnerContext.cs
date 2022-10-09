using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestCaseRunner"/>.
/// </summary>
public class XunitTestCaseRunnerContext : TestCaseRunnerContext<IXunitTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestCaseRunnerContext"/> record.
	/// </summary>
	public XunitTestCaseRunnerContext(
		IXunitTestCase testCase,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		string displayName,
		string? skipReason,
		ExplicitOption explicitOption,
		Type testClass,
		object?[] constructorArguments,
		MethodInfo testMethod,
		object?[]? testMethodArguments,
		IReadOnlyCollection<BeforeAfterTestAttribute> beforeAfterTestAttributes) :
			base(testCase, explicitOption, messageBus, aggregator, cancellationTokenSource)
	{
		DisplayName = displayName;
		SkipReason = skipReason;
		TestClass = testClass;
		ConstructorArguments = constructorArguments;
		TestMethod = testMethod;
		TestMethodArguments = testMethodArguments;
		BeforeAfterTestAttributes = beforeAfterTestAttributes;
	}

	/// <summary>
	/// Gets the list of <see cref="BeforeAfterTestAttribute"/> instances for this test case.
	/// </summary>
	public IReadOnlyCollection<BeforeAfterTestAttribute> BeforeAfterTestAttributes { get; }

	/// <summary>
	/// Gets the arguments to pass to the constructor of the test class when creating it.
	/// </summary>
	public object?[] ConstructorArguments { get; }

	/// <summary>
	/// Gets the display name of the test case.
	/// </summary>
	public string DisplayName { get; }

	/// <summary>
	/// Gets the statically specified skip reason for the test. Note that this only covers values
	/// passed via <see cref="FactAttribute.Skip"/>, and not dynamically skipped tests.
	/// </summary>
	public string? SkipReason { get; }

	/// <summary>
	/// Gets the type that this test case belongs to.
	/// </summary>
	public Type TestClass { get; }

	/// <summary>
	/// Gets the test method that this test case belongs to.
	/// </summary>
	public MethodInfo TestMethod { get; }

	/// <summary>
	/// Gets the arguments to pass to the test method of the test case.
	/// </summary>
	public object?[]? TestMethodArguments { get; }
}
