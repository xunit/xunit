using System;
using System.Linq;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public class DelegatingSummarySinkTests
{
	readonly XunitProjectAssembly assembly;
	readonly _ITestFrameworkDiscoveryOptions discoveryOptions;
	readonly _ITestFrameworkExecutionOptions executionOptions;
	readonly SpyMessageSink innerSink;

	public DelegatingSummarySinkTests()
	{
		var project = new XunitProject();
		assembly = new XunitProjectAssembly(project);

		discoveryOptions = _TestFrameworkOptions.ForDiscovery();

		executionOptions = _TestFrameworkOptions.ForExecution();

		innerSink = SpyMessageSink.Capture();
	}

	public class Cancellation : DelegatingSummarySinkTests
	{
		readonly _MessageSinkMessage testMessage;

		public Cancellation()
		{
			testMessage = new _MessageSinkMessage();
		}

		[Fact]
		public void ReturnsFalseWhenCancellationThunkIsTrue()
		{
			var sink = new DelegatingSummarySink(assembly, discoveryOptions, executionOptions, AppDomainOption.NotAvailable, false, innerSink, () => true);

			var result = sink.OnMessage(testMessage);

			Assert.False(result);
		}

		[Fact]
		public void ReturnsTrueWhenCancellationThunkIsFalse()
		{
			var sink = new DelegatingSummarySink(assembly, discoveryOptions, executionOptions, AppDomainOption.NotAvailable, false, innerSink, () => false);

			var result = sink.OnMessage(testMessage);

			Assert.True(result);
		}
	}

	public class DiscoveryMessages : DelegatingSummarySinkTests
	{
		[Theory]
		[InlineData(AppDomainOption.Enabled, true)]
		[InlineData(AppDomainOption.Disabled, false)]
		public void ConvertsDiscoveryStarting(
			AppDomainOption appDomain,
			bool shadowCopy)
		{
			var sink = new DelegatingSummarySink(assembly, discoveryOptions, executionOptions, appDomain, shadowCopy, innerSink, () => true);
			var testMessage = TestData.DiscoveryStarting();

			sink.OnMessage(testMessage);

			var result = Assert.Single(innerSink.Messages.OfType<TestAssemblyDiscoveryStarting>());
			Assert.Same(assembly, result.Assembly);
			Assert.Equal(appDomain, result.AppDomain);
			Assert.Same(discoveryOptions, result.DiscoveryOptions);
			Assert.Equal(shadowCopy, result.ShadowCopy);
		}

		[Fact]
		public void ConvertsDiscoveryComplete()
		{
			var sink = new DelegatingSummarySink(assembly, discoveryOptions, executionOptions, AppDomainOption.NotAvailable, false, innerSink, () => true);
			var testMessage = TestData.DiscoveryComplete(testCasesToRun: 42);

			sink.OnMessage(testMessage);

			var result = Assert.Single(innerSink.Messages.OfType<TestAssemblyDiscoveryFinished>());
			Assert.Same(assembly, result.Assembly);
			Assert.Same(discoveryOptions, result.DiscoveryOptions);
			Assert.Equal(42, result.TestCasesToRun);
		}
	}

	public class ExecutionMessages : DelegatingSummarySinkTests
	{
		[Fact]
		public void ConvertsTestAssemblyStarting()
		{
			var sink = new DelegatingSummarySink(assembly, discoveryOptions, executionOptions, AppDomainOption.NotAvailable, false, innerSink, () => true);
			var testMessage = TestData.TestAssemblyStarting();

			sink.OnMessage(testMessage);

			var result = Assert.Single(innerSink.Messages.OfType<TestAssemblyExecutionStarting>());
			Assert.Same(assembly, result.Assembly);
			Assert.Same(executionOptions, result.ExecutionOptions);
		}

		[Fact]
		public void ConvertsTestAssemblyFinished()
		{
			var sink = new DelegatingSummarySink(assembly, discoveryOptions, executionOptions, AppDomainOption.NotAvailable, false, innerSink, () => true);
			var testMessage = TestData.TestAssemblyFinished();

			sink.OnMessage(testMessage);

			var result = Assert.Single(innerSink.Messages.OfType<TestAssemblyExecutionFinished>());
			Assert.Same(assembly, result.Assembly);
			Assert.Same(executionOptions, result.ExecutionOptions);
			Assert.Same(sink.ExecutionSummary, result.ExecutionSummary);
		}

		[Theory]
		[InlineData(typeof(_ErrorMessage))]
		public void CountsErrors(Type errorType)
		{
			var sink = new DelegatingSummarySink(assembly, discoveryOptions, executionOptions, AppDomainOption.NotAvailable, false, innerSink, () => true);
			var error = (_MessageSinkMessage)Activator.CreateInstance(errorType)!;
			var finished = TestData.TestAssemblyFinished();  // Need finished message to finalized the error count

			sink.OnMessage(error);
			sink.OnMessage(finished);

			Assert.Equal(1, sink.ExecutionSummary.Errors);
		}
	}
}
