using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base context type for <see cref="XunitDelayEnumeratedTestCaseRunnerBase{TContext, TTestCase}"/>.
/// </summary>
/// <typeparam name="TTestCase">The test case type</typeparam>
/// <param name="testCase">The test case</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="displayName">The display name of the test case</param>
/// <param name="skipReason">The skip reason, if the test case is being skipped</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="constructorArguments">The constructor arguments for the test class</param>
public class XunitDelayEnumeratedTestCaseRunnerContext<TTestCase>(
	TTestCase testCase,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	string displayName,
	string? skipReason,
	ExplicitOption explicitOption,
	object?[] constructorArguments) :
		XunitTestCaseRunnerContext<TTestCase>(testCase, messageBus, aggregator, cancellationTokenSource, displayName, skipReason, explicitOption, constructorArguments, [])
			where TTestCase : class, IXunitDelayEnumeratedTestCase
{
	/// <summary>
	/// Gets a container to place tests found during discovery.
	/// </summary>
	public List<(IXunitTest Test, string? SkipReason)> DiscoveredTests { get; } = [];

	/// <summary>
	/// Gets or sets the exception that occurred during data discovery. When this is set, running the test case will result in
	/// a single failure related to the discovery exception.
	/// </summary>
	public Exception? DiscoveryException { get; set; }

	/// <summary>
	/// Gets the disposal tracker used to ensure that discovered data is properly disposed of.
	/// </summary>
	public DisposalTracker DisposalTracker { get; } = new();
}
