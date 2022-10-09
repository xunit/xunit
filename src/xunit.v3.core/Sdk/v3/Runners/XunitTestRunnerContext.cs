using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestRunner"/>.
/// </summary>
public class XunitTestRunnerContext : TestRunnerContext
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestRunnerContext"/> class.
	/// </summary>
	public XunitTestRunnerContext(
		_ITest test,
		IMessageBus messageBus,
		Type testClass,
		object?[] constructorArguments,
		MethodInfo testMethod,
		object?[]? testMethodArguments,
		string? skipReason,
		ExplicitOption explicitOption,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		IReadOnlyCollection<BeforeAfterTestAttribute> beforeAfterTestAttributes) :
			base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, skipReason, explicitOption, aggregator, cancellationTokenSource)
	{
		BeforeAfterTestAttributes = Guard.ArgumentNotNull(beforeAfterTestAttributes);
	}

	/// <summary>
	/// Gets the collection of <see cref="BeforeAfterTestAttribute"/> used for this test.
	/// </summary>
	public IReadOnlyCollection<BeforeAfterTestAttribute> BeforeAfterTestAttributes { get; }
}
