using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestInvoker"/>.
/// </summary>
public class XunitTestInvokerContext : TestInvokerContext
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestInvokerContext"/> class.
	/// </summary>
	public XunitTestInvokerContext(
		_ITest test,
		Type testClass,
		object?[] constructorArguments,
		MethodInfo testMethod,
		object?[]? testMethodArguments,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		IReadOnlyCollection<BeforeAfterTestAttribute> beforeAfterTestAttributes) :
			base(test, testClass, constructorArguments, testMethod, testMethodArguments, explicitOption, messageBus, aggregator, cancellationTokenSource)
	{
		BeforeAfterTestAttributes = Guard.ArgumentNotNull(beforeAfterTestAttributes);
	}

	/// <summary>
	/// Gets or sets the collection of <see cref="BeforeAfterTestAttribute"/> used for this test. Before
	/// the test is invoked, this list will contain the list of discovered attributes; after the test has
	/// been invoked, this list will only contain the attributes that were successfully initialized. The
	/// order is irrelevant in its original form; for the list of initialized attributes, iteration will
	/// return the attributes in the reverse order that they were initialized (so that last initialized
	/// will be first cleaned up).
	/// </summary>
	public IReadOnlyCollection<BeforeAfterTestAttribute> BeforeAfterTestAttributes { get; set; }
}
