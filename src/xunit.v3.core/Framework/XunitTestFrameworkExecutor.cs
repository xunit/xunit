using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

// TODO: Need to acceptance test this via Xunit3. See Xunit2AcceptanceTests.cs for examples.

namespace Xunit.v3;

/// <summary>
/// The implementation of <see cref="ITestFrameworkExecutor"/> that supports execution
/// of unit tests linked against xunit.v3.core.dll.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="XunitTestFrameworkExecutor"/> class.
/// </remarks>
/// <param name="testAssembly">The test assembly.</param>
public class XunitTestFrameworkExecutor(IXunitTestAssembly testAssembly) :
	TestFrameworkExecutor<IXunitTestCase>(testAssembly)
{
	readonly Lazy<XunitTestFrameworkDiscoverer> discoverer = new(() => new(testAssembly));

	/// <summary>
	/// Gets the test assembly that contains the test.
	/// </summary>
	protected new IXunitTestAssembly TestAssembly { get; } = Guard.ArgumentNotNull(testAssembly);

	/// <inheritdoc/>
	protected override ITestFrameworkDiscoverer CreateDiscoverer() =>
		discoverer.Value;

	/// <inheritdoc/>
	public override async ValueTask RunTestCases(
		IReadOnlyCollection<IXunitTestCase> testCases,
		IMessageSink executionMessageSink,
		ITestFrameworkExecutionOptions executionOptions) =>
			await XunitTestAssemblyRunner.Instance.RunAsync(TestAssembly, testCases, executionMessageSink, executionOptions);
}
