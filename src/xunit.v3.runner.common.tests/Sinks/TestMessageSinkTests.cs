#pragma warning disable xUnit3002  // We use the concrete message types here because we Activator.Create them

using System;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;

public class TestMessageSinkTests
{
	[Theory]

	// Diagnostics
	[InlineData(typeof(DiagnosticMessage))]
	[InlineData(typeof(ErrorMessage))]

	// Discovery
	[InlineData(typeof(DiscoveryComplete))]
	[InlineData(typeof(DiscoveryStarting))]
	[InlineData(typeof(TestCaseDiscovered))]

	// Execution
	[InlineData(typeof(AfterTestFinished))]
	[InlineData(typeof(AfterTestStarting))]
	[InlineData(typeof(BeforeTestFinished))]
	[InlineData(typeof(BeforeTestStarting))]
	[InlineData(typeof(TestAssemblyCleanupFailure))]
	[InlineData(typeof(TestAssemblyFinished))]
	[InlineData(typeof(TestAssemblyStarting))]
	[InlineData(typeof(TestCaseCleanupFailure))]
	[InlineData(typeof(TestCaseFinished))]
	[InlineData(typeof(TestCaseStarting))]
	[InlineData(typeof(TestClassCleanupFailure))]
	[InlineData(typeof(TestClassConstructionFinished))]
	[InlineData(typeof(TestClassConstructionStarting))]
	[InlineData(typeof(TestClassDisposeFinished))]
	[InlineData(typeof(TestClassDisposeStarting))]
	[InlineData(typeof(TestClassFinished))]
	[InlineData(typeof(TestClassStarting))]
	[InlineData(typeof(TestCollectionCleanupFailure))]
	[InlineData(typeof(TestCollectionFinished))]
	[InlineData(typeof(TestCollectionStarting))]
	[InlineData(typeof(TestCleanupFailure))]
	[InlineData(typeof(TestFailed))]
	[InlineData(typeof(TestFinished))]
	[InlineData(typeof(TestMethodCleanupFailure))]
	[InlineData(typeof(TestMethodFinished))]
	[InlineData(typeof(TestMethodStarting))]
	[InlineData(typeof(TestNotRun))]
	[InlineData(typeof(TestOutput))]
	[InlineData(typeof(TestPassed))]
	[InlineData(typeof(TestSkipped))]
	[InlineData(typeof(TestStarting))]

	// Runner
	[InlineData(typeof(TestAssemblyExecutionStarting))]
	[InlineData(typeof(TestAssemblyExecutionFinished))]
	[InlineData(typeof(TestAssemblyDiscoveryStarting))]
	[InlineData(typeof(TestAssemblyDiscoveryFinished))]
	[InlineData(typeof(TestExecutionSummaries))]
	public void ProcessesVisitorTypes(Type type)
	{
		var message = Activator.CreateInstance(type);
		Assert.NotNull(message);
		var typedMessage = Assert.IsAssignableFrom<IMessageSinkMessage>(message);
		var sink = new SpyTestMessageSink();

		sink.OnMessage(typedMessage);

		var msg = Assert.Single(sink.Calls);
		Assert.Equal(type.Name, msg);
	}
}
