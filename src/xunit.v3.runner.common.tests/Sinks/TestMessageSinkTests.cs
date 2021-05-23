using System;
using Xunit;
using Xunit.Runner.Common;
using Xunit.v3;

public class TestMessageSinkTests
{
	public static TheoryData<Type> ProcessesVisitorTypesData = new()
	{
		// Diagnostics
		typeof(_DiagnosticMessage),
		typeof(_ErrorMessage),
		// Discovery
		typeof(_DiscoveryComplete),
		typeof(_DiscoveryStarting),
		typeof(_TestCaseDiscovered),
		// Execution
		typeof(_AfterTestFinished),
		typeof(_AfterTestStarting),
		typeof(_BeforeTestFinished),
		typeof(_BeforeTestStarting),
		typeof(_TestAssemblyCleanupFailure),
		typeof(_TestAssemblyFinished),
		typeof(_TestAssemblyStarting),
		typeof(_TestCaseCleanupFailure),
		typeof(_TestCaseFinished),
		typeof(_TestCaseStarting),
		typeof(_TestClassCleanupFailure),
		typeof(_TestClassConstructionFinished),
		typeof(_TestClassConstructionStarting),
		typeof(_TestClassDisposeFinished),
		typeof(_TestClassDisposeStarting),
		typeof(_TestClassFinished),
		typeof(_TestClassStarting),
		typeof(_TestCollectionCleanupFailure),
		typeof(_TestCollectionFinished),
		typeof(_TestCollectionStarting),
		typeof(_TestCleanupFailure),
		typeof(_TestFailed),
		typeof(_TestFinished),
		typeof(_TestMethodCleanupFailure),
		typeof(_TestMethodFinished),
		typeof(_TestMethodStarting),
		typeof(_TestOutput),
		typeof(_TestPassed),
		typeof(_TestSkipped),
		typeof(_TestStarting),
		// Runner
		typeof(TestAssemblyExecutionStarting),
		typeof(TestAssemblyExecutionFinished),
		typeof(TestAssemblyDiscoveryStarting),
		typeof(TestAssemblyDiscoveryFinished),
		typeof(TestExecutionSummaries),
	};

	[Theory]
	[MemberData(nameof(ProcessesVisitorTypesData), DisableDiscoveryEnumeration = true)]
	public void ProcessesVisitorTypes(Type type)
	{
		var message = Activator.CreateInstance(type);
		Assert.NotNull(message);
		var typedMessage = Assert.IsAssignableFrom<_MessageSinkMessage>(message);
		var sink = new SpyTestMessageSink();

		sink.OnMessage(typedMessage);

		var msg = Assert.Single(sink.Calls);
		Assert.Equal(type.Name, msg);
	}
}
