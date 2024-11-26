using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestCaseRunnerTests
{
	public class Messages
	{
		[Fact]
		public async ValueTask NoException()
		{
			var runner = new TestableTestCaseRunner();

			await runner.Run(exception: null);

			Assert.Collection(
				runner.MessageBus.Messages,
				message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
				// We're delegating to RunTest (abstract) so no lower messages
				message => Assert.IsAssignableFrom<ITestCaseFinished>(message)
			);
		}

		[Fact]
		public async ValueTask WithException_DispatchesFailureMessages()
		{
			var runner = new TestableTestCaseRunner();
			var exception = Record.Exception(ThrowException);

			await runner.Run(exception);

			Assert.Collection(
				runner.MessageBus.Messages,
				message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
				// FailTest sends all the failure messages
				message => Assert.IsAssignableFrom<ITestStarting>(message),
				message =>
				{
					var failed = Assert.IsAssignableFrom<ITestFailed>(message);
					Assert.Equal(new[] { -1 }, failed.ExceptionParentIndices);
					Assert.Equal(new[] { "System.DivideByZeroException" }, failed.ExceptionTypes);
					Assert.Equal(new[] { "Attempted to divide by zero." }, failed.Messages);
					Assert.NotEmpty(failed.StackTraces.Single()!);
				},
				message => Assert.IsAssignableFrom<ITestFinished>(message),
				message => Assert.IsAssignableFrom<ITestCaseFinished>(message)
			);
		}

		static void ThrowException() =>
			throw new DivideByZeroException();
	}

	class TestableTestCaseRunner(ITest? test = null) :
		TestCaseRunner<TestableTestCaseRunnerContext, ITestCase, ITest>
	{
		public readonly ExceptionAggregator Aggregator = new();
		public readonly SpyMessageBus MessageBus = new();
		public readonly ITest Test = test ?? Mocks.Test();
		public readonly CancellationTokenSource TokenSource = new();

		public async ValueTask<RunSummary> Run(Exception? exception)
		{
			await using var ctxt = new TestableTestCaseRunnerContext(Test.TestCase, Test, ExplicitOption.Off, MessageBus, Aggregator, TokenSource);
			await ctxt.InitializeAsync();

			if (exception is not null)
				ctxt.Aggregator.Add(exception);

			return await Run(ctxt);
		}

		public RunSummary RunTest__Result = new();

		protected override ValueTask<RunSummary> RunTest(
			TestableTestCaseRunnerContext ctxt,
			ITest test) =>
				new(RunTest__Result);
	}

	class TestableTestCaseRunnerContext(
		ITestCase testCase,
		ITest test,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource) :
			TestCaseRunnerContext<ITestCase, ITest>(testCase, explicitOption, messageBus, aggregator, cancellationTokenSource)
	{
		public override IReadOnlyCollection<ITest> Tests { get; } = [test];
	}
}
