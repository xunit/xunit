using System;
using System.Linq;
using System.Reflection;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.Common;
using Xunit.Runner.v2;
using Xunit.v3;

public class TestMessageSinkTests
{
	static readonly MethodInfo forMethodGeneric = typeof(Substitute).GetMethods().Single(m => m.Name == nameof(Substitute.For) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1);

	[Theory]
	// Diagnostics
	[InlineData(typeof(_DiagnosticMessage))]
	[InlineData(typeof(_ErrorMessage))]
	// Discovery
	[InlineData(typeof(_DiscoveryComplete))]
	[InlineData(typeof(_DiscoveryStarting))]
	[InlineData(typeof(_TestCaseDiscovered))]
	// Execution
	[InlineData(typeof(_AfterTestFinished))]
	[InlineData(typeof(_AfterTestStarting))]
	[InlineData(typeof(_BeforeTestFinished))]
	[InlineData(typeof(_BeforeTestStarting))]
	[InlineData(typeof(_TestAssemblyCleanupFailure))]
	[InlineData(typeof(_TestAssemblyFinished))]
	[InlineData(typeof(_TestAssemblyStarting))]
	[InlineData(typeof(_TestCaseCleanupFailure))]
	[InlineData(typeof(_TestCaseFinished))]
	[InlineData(typeof(_TestCaseStarting))]
	[InlineData(typeof(_TestClassCleanupFailure))]
	[InlineData(typeof(_TestClassConstructionFinished))]
	[InlineData(typeof(_TestClassConstructionStarting))]
	[InlineData(typeof(_TestClassDisposeFinished))]
	[InlineData(typeof(_TestClassDisposeStarting))]
	[InlineData(typeof(_TestClassFinished))]
	[InlineData(typeof(_TestClassStarting))]
	[InlineData(typeof(_TestCollectionCleanupFailure))]
	[InlineData(typeof(_TestCollectionFinished))]
	[InlineData(typeof(_TestCollectionStarting))]
	[InlineData(typeof(_TestCleanupFailure))]
	[InlineData(typeof(_TestFailed))]
	[InlineData(typeof(_TestFinished))]
	[InlineData(typeof(_TestMethodCleanupFailure))]
	[InlineData(typeof(_TestMethodFinished))]
	[InlineData(typeof(_TestMethodStarting))]
	[InlineData(typeof(_TestOutput))]
	[InlineData(typeof(_TestPassed))]
	[InlineData(typeof(_TestSkipped))]
	[InlineData(typeof(_TestStarting))]
	// Runner
	[InlineData(typeof(TestAssemblyExecutionStarting))]
	[InlineData(typeof(TestAssemblyExecutionFinished))]
	[InlineData(typeof(TestAssemblyDiscoveryStarting))]
	[InlineData(typeof(TestAssemblyDiscoveryFinished))]
	[InlineData(typeof(TestExecutionSummaries))]
	public void ProcessesVisitorTypes(Type type)
	{
		var forMethod = forMethodGeneric.MakeGenericMethod(type);
		var substitute = (IMessageSinkMessage)forMethod.Invoke(null, new object[] { new object[0] })!;
		var sink = new SpyTestMessageSink();

		sink.OnMessage(substitute);

		Assert.Collection(
			sink.Calls,
			msg => Assert.Equal(type.Name, msg)
		);
	}
}
