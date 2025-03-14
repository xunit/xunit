using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestRunnerBaseTests
{
	public class InvocationsAndMessages
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public static async ValueTask Passed(bool cancel)
		{
			var now = DateTimeOffset.UtcNow;
			var testClassInstance = new object();

			var runner = new TestableTestRunnerBase
			{
				GetAttachments__Result = new Dictionary<string, TestAttachment> { ["foo"] = TestAttachment.Create("bar") },
				GetTestOutput__Result = "the output",
				GetWarnings__Result = ["warning1", "warning2"],
				OnTestPassed__Result = !cancel,
			};

			var summary = await runner.Run();

			VerifyRunSummary(summary);
			Assert.Equal(cancel, runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"RunTest",
				"GetTestOutput",
				"GetWarnings",
				"GetAttachments",
				"OnTestPassed(output: \"the output\")",
				// OnTestCleanupFailure
				"OnTestFinished(output: \"the output\", attachments: [[\"foo\"] = s:bar])",
				// OnError
			}, runner.Invocations);

			var starting_StartTime = DateTimeOffset.MaxValue;
			var passed_ExecutionTime = -1m;
			var passed_FinishTime = DateTimeOffset.MinValue;

			Assert.Collection(
				runner.MessageBus.Messages,
				message =>
				{
					var starting = Assert.IsAssignableFrom<ITestStarting>(message);
					verifyTestMessage(starting);

					starting_StartTime = starting.StartTime;
					Assert.False(starting.Explicit);
					Assert.True(starting_StartTime >= now, $"Expected {starting_StartTime} >= {now}");
					Assert.Equal("test-display-name", starting.TestDisplayName);
					Assert.Equal(0, starting.Timeout);
				},
				message =>
				{
					var passed = Assert.IsAssignableFrom<ITestPassed>(message);
					verifyTestMessage(passed);

					passed_ExecutionTime = passed.ExecutionTime;
					passed_FinishTime = passed.FinishTime;
					Assert.True(passed_ExecutionTime > 0m);
					Assert.True(passed_FinishTime >= starting_StartTime, $"Expected {passed_FinishTime} >= {starting_StartTime}");
					Assert.Equal("the output", passed.Output);
					Assert.NotNull(passed.Warnings);
					Assert.Equal(["warning1", "warning2"], passed.Warnings);
				},
				message =>
				{
					var finished = Assert.IsAssignableFrom<ITestFinished>(message);
					verifyTestMessage(finished);

					var attachment = Assert.Single(finished.Attachments);
					Assert.Equal("foo", attachment.Key);
					Assert.Equal(TestAttachmentType.String, attachment.Value.AttachmentType);
					Assert.Equal("bar", attachment.Value.AsString());
					Assert.Equal(passed_ExecutionTime, finished.ExecutionTime);
					Assert.True(finished.FinishTime >= passed_FinishTime, $"Expected {finished.FinishTime} >= {passed_FinishTime}");
					Assert.Equal("the output", finished.Output);
					Assert.NotNull(finished.Warnings);
					Assert.Equal(["warning1", "warning2"], finished.Warnings);
				}
			);

			void verifyTestMessage(ITestMessage message)
			{
				Assert.Equal(runner.Test.TestCase.TestCollection.TestAssembly.UniqueID, message.AssemblyUniqueID);
				Assert.Equal(runner.Test.TestCase.UniqueID, message.TestCaseUniqueID);
				Assert.Equal(runner.Test.TestCase.TestClass?.UniqueID, message.TestClassUniqueID);
				Assert.Equal(runner.Test.TestCase.TestCollection.UniqueID, message.TestCollectionUniqueID);
				Assert.Equal(runner.Test.TestCase.TestMethod?.UniqueID, message.TestMethodUniqueID);
				Assert.Equal(runner.Test.UniqueID, message.TestUniqueID);
			}
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public static async ValueTask Failed(bool cancel)
		{
			var runner = new TestableTestRunnerBase
			{
				GetTestOutput__Result = "the output",
				OnTestFailed__Result = !cancel,
				RunTest__Lambda = () => Assert.True(false),
			};

			var summary = await runner.Run();

			VerifyRunSummary(summary, failed: 1);
			Assert.Equal(cancel, runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"RunTest",
				"GetTestOutput",
				"GetWarnings",
				"GetAttachments",
				"OnTestFailed(exception: typeof(TrueException), output: \"the output\")",
				// OnTestCleanupFailure
				"OnTestFinished(output: \"the output\")",
				// OnError
			}, runner.Invocations);
			Assert.Collection(
				runner.MessageBus.Messages,
				message => Assert.IsAssignableFrom<ITestStarting>(message),
				message =>
				{
					var failed = Assert.IsAssignableFrom<ITestFailed>(message);
					Assert.Equal(FailureCause.Assertion, failed.Cause);
					Assert.Equal(-1, failed.ExceptionParentIndices.Single());
					Assert.Equal(typeof(TrueException).FullName, failed.ExceptionTypes.Single());
					Assert.Equal($"Assert.True() Failure{Environment.NewLine}Expected: True{Environment.NewLine}Actual:   False", failed.Messages.Single());
					Assert.Equal("the output", failed.Output);
					Assert.NotEmpty(failed.StackTraces);
				},
				message => Assert.IsAssignableFrom<ITestFinished>(message)
			);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public static async ValueTask Skipped_Static(bool cancel)
		{
			var runner = new TestableTestRunnerBase { OnTestSkipped__Result = !cancel, };

			var summary = await runner.Run("Don't run me");

			VerifyRunSummary(summary, skipped: 1);
			Assert.Equal(cancel, runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				// RunTest
				"GetTestOutput",
				"GetWarnings",
				"GetAttachments",
				"OnTestSkipped(reason: \"Don't run me\", output: \"\")",
				// OnTestCleanupFailure
				"OnTestFinished(output: \"\")",
				// OnError
			}, runner.Invocations);
			Assert.Collection(
				runner.MessageBus.Messages,
				message => Assert.IsAssignableFrom<ITestStarting>(message),
				message =>
				{
					var skipped = Assert.IsAssignableFrom<ITestSkipped>(message);
					Assert.Equal("Don't run me", skipped.Reason);
				},
				message => Assert.IsAssignableFrom<ITestFinished>(message)
			);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public static async ValueTask NotRun(bool cancel)
		{
			var runner = new TestableTestRunnerBase
			{
				OnTestNotRun__Result = !cancel,
				ShouldTestRun__Result = false,
			};

			var summary = await runner.Run();

			VerifyRunSummary(summary, notRun: 1);
			Assert.Equal(cancel, runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				// RunTest
				"GetTestOutput",
				"GetWarnings",
				"GetAttachments",
				"OnTestNotRun(output: \"\")",
				// OnTestCleanupFailure
				"OnTestFinished(output: \"\")",
				// OnError
			}, runner.Invocations);
			Assert.Collection(
				runner.MessageBus.Messages,
				message => Assert.IsAssignableFrom<ITestStarting>(message),
				message => Assert.IsAssignableFrom<ITestNotRun>(message),
				message => Assert.IsAssignableFrom<ITestFinished>(message)
			);
		}
	}

	public class ExceptionHandling
	{
		[Fact]
		public static async ValueTask GetAttachments()
		{
			var runner = new TestableTestRunnerBase { GetAttachments__Lambda = () => throw new DivideByZeroException() };

			var summary = await runner.Run();

			VerifyRunSummary(summary, failed: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"RunTest",
				"GetTestOutput",
				"GetWarnings",
				"GetAttachments",
				"OnTestFailed(exception: typeof(DivideByZeroException), output: \"\")",
				// OnTestCleanupFailure
				"OnTestFinished(output: \"\")",
				// OnError
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask GetTestOutput()
		{
			var runner = new TestableTestRunnerBase { GetTestOutput__Lambda = () => throw new DivideByZeroException() };

			var summary = await runner.Run();

			VerifyRunSummary(summary, failed: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"RunTest",
				"GetTestOutput",
				"GetWarnings",
				"GetAttachments",
				"OnTestFailed(exception: typeof(DivideByZeroException), output: \"\")",
				// OnTestCleanupFailure
				"OnTestFinished(output: \"\")",
				// OnError
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask GetWarnings()
		{
			var runner = new TestableTestRunnerBase { GetWarnings__Lambda = () => throw new DivideByZeroException() };

			var summary = await runner.Run();

			VerifyRunSummary(summary, failed: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"RunTest",
				"GetTestOutput",
				"GetWarnings",
				"GetAttachments",
				"OnTestFailed(exception: typeof(DivideByZeroException), output: \"\")",
				// OnTestCleanupFailure
				"OnTestFinished(output: \"\")",
				// OnError
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask OnTestCleanupFailure()
		{
			// Need to record an exception into the aggregator for OnTestFinished to trigger OnTestCleanupFailure
			var runner = new TestableTestRunnerBase { OnTestCleanupFailure__Lambda = () => throw new DivideByZeroException() };
			runner.OnTestFinished__Lambda = () => runner.Aggregator.Add(new ArgumentException());

			var summary = await runner.Run();

			VerifyRunSummary(summary);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"RunTest",
				"GetTestOutput",
				"GetWarnings",
				"GetAttachments",
				"OnTestPassed(output: \"\")",
				"OnTestCleanupFailure(exception: typeof(ArgumentException))",
				"OnTestFinished(output: \"\")",
				"OnError(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
			var errorMessage = Assert.Single(runner.MessageBus.Messages.OfType<IErrorMessage>());
			Assert.Equal(-1, errorMessage.ExceptionParentIndices.Single());
			Assert.Equal("System.DivideByZeroException", errorMessage.ExceptionTypes.Single());
			Assert.Equal("Attempted to divide by zero.", errorMessage.Messages.Single());
			Assert.NotEmpty(errorMessage.StackTraces.Single()!);
		}

		[Fact]
		public static async ValueTask OnTestFailed()
		{
			var runner = new TestableTestRunnerBase
			{
				RunTest__Lambda = () => Assert.True(false),
				OnTestFailed__Lambda = () => throw new DivideByZeroException(),
			};

			var summary = await runner.Run();

			VerifyRunSummary(summary, failed: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"RunTest",
				"GetTestOutput",
				"GetWarnings",
				"GetAttachments",
				"OnTestFailed(exception: typeof(TrueException), output: \"\")",
				"OnTestCleanupFailure(exception: typeof(DivideByZeroException))",
				"OnTestFinished(output: \"\")",
				// OnError
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask OnTestFinished()
		{
			var runner = new TestableTestRunnerBase();
			runner.OnTestFinished__Lambda = () => runner.Aggregator.Add(new DivideByZeroException());

			var summary = await runner.Run();

			VerifyRunSummary(summary);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"RunTest",
				"GetTestOutput",
				"GetWarnings",
				"GetAttachments",
				"OnTestPassed(output: \"\")",
				"OnTestCleanupFailure(exception: typeof(DivideByZeroException))",
				"OnTestFinished(output: \"\")",
				// OnError
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask OnTestNotRun()
		{
			var runner = new TestableTestRunnerBase
			{
				OnTestNotRun__Lambda = () => throw new DivideByZeroException(),
				ShouldTestRun__Result = false,
			};

			var summary = await runner.Run();

			VerifyRunSummary(summary, notRun: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				// RunTest
				"GetTestOutput",
				"GetWarnings",
				"GetAttachments",
				"OnTestNotRun(output: \"\")",
				"OnTestCleanupFailure(exception: typeof(DivideByZeroException))",
				"OnTestFinished(output: \"\")",
				// OnError
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask OnTestPassed()
		{
			var runner = new TestableTestRunnerBase { OnTestPassed__Lambda = () => throw new DivideByZeroException() };

			var summary = await runner.Run();

			VerifyRunSummary(summary);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"RunTest",
				"GetTestOutput",
				"GetWarnings",
				"GetAttachments",
				"OnTestPassed(output: \"\")",
				"OnTestCleanupFailure(exception: typeof(DivideByZeroException))",
				"OnTestFinished(output: \"\")",
				// OnError
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask OnTestSkipped()
		{
			var runner = new TestableTestRunnerBase { OnTestSkipped__Lambda = () => throw new DivideByZeroException(), };

			var summary = await runner.Run("Don't run me");

			VerifyRunSummary(summary, skipped: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				// RunTest
				"GetTestOutput",
				"GetWarnings",
				"GetAttachments",
				"OnTestSkipped(reason: \"Don't run me\", output: \"\")",
				"OnTestCleanupFailure(exception: typeof(DivideByZeroException))",
				"OnTestFinished(output: \"\")",
				// OnError
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask OnTestStarting()
		{
			var runner = new TestableTestRunnerBase { OnTestStarting__Lambda = () => throw new DivideByZeroException() };

			var summary = await runner.Run();

			VerifyRunSummary(summary, total: 1, failed: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				// ShouldTestRun
				// RunTest
				"GetTestOutput",
				"GetWarnings",
				"GetAttachments",
				"OnTestFailed(exception: typeof(DivideByZeroException), output: \"\")",
				// OnTestCleanupFailure
				"OnTestFinished(output: \"\")",
				// OnError
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask ShouldTestRun()
		{
			var runner = new TestableTestRunnerBase { ShouldTestRun__Lambda = () => throw new DivideByZeroException() };

			var summary = await runner.Run();

			VerifyRunSummary(summary, failed: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				// RunTest
				"GetTestOutput",
				"GetWarnings",
				"GetAttachments",
				"OnTestFailed(exception: typeof(DivideByZeroException), output: \"\")",
				// OnTestCleanupFailure
				"OnTestFinished(output: \"\")",
				// OnError
			}, runner.Invocations);
		}
	}

	// We test the standard handlers (OnTestPassed, OnTestFailed, etc.) via theory in Invocations above
	// so these tests are the ancillary cancellable handlers (starting, finished, and cleanup failure).
	public class Cancellation
	{
		[Fact]
		public static async ValueTask OnError()
		{
			// Need to record an exception into the aggregator for OnTestFinished to trigger OnTestCleanupFailure
			var runner = new TestableTestRunnerBase
			{
				OnError__Result = false,
				OnTestCleanupFailure__Lambda = () => throw new DivideByZeroException(),
			};
			runner.OnTestFinished__Lambda = () => runner.Aggregator.Add(new ArgumentException());

			var summary = await runner.Run();

			VerifyRunSummary(summary, total: 1);
			Assert.True(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"RunTest",
				"GetTestOutput",
				"GetWarnings",
				"GetAttachments",
				"OnTestPassed(output: \"\")",
				"OnTestCleanupFailure(exception: typeof(ArgumentException))",
				"OnTestFinished(output: \"\")",
				"OnError(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
		}


		[Fact]
		public static async ValueTask OnTestCleanupFailure()
		{
			// Need to record an exception into the aggregator for OnTestFinished to trigger OnTestCleanupFailure
			var runner = new TestableTestRunnerBase { OnTestCleanupFailure__Result = false };
			runner.OnTestFinished__Lambda = () => runner.Aggregator.Add(new ArgumentException());

			var summary = await runner.Run();

			VerifyRunSummary(summary, total: 1);
			Assert.True(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"RunTest",
				"GetTestOutput",
				"GetWarnings",
				"GetAttachments",
				"OnTestPassed(output: \"\")",
				"OnTestCleanupFailure(exception: typeof(ArgumentException))",
				"OnTestFinished(output: \"\")",
				// OnError
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask OnTestFinished()
		{
			var runner = new TestableTestRunnerBase { OnTestFinished__Result = false };

			var summary = await runner.Run();

			VerifyRunSummary(summary);
			Assert.True(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"RunTest",
				"GetTestOutput",
				"GetWarnings",
				"GetAttachments",
				"OnTestPassed(output: \"\")",
				// OnTestCleanupFailure
				"OnTestFinished(output: \"\")",
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask OnTestStarting()
		{
			var runner = new TestableTestRunnerBase { OnTestStarting__Result = false };

			var summary = await runner.Run();

			VerifyRunSummary(summary, total: 0);
			Assert.True(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				// ShouldTestRun
				// RunTest
				// GetTestOutput
				// GetWarnings
				// GetAttachments
				// OnTestXxx
				// OnTestCleanupFailure
				"OnTestFinished(output: \"\")",
			}, runner.Invocations);
		}
	}

	static void VerifyRunSummary(
		RunSummary summary,
		int total = 1,
		int failed = 0,
		int notRun = 0,
		int skipped = 0) =>
			Assert.Equivalent(new { Total = total, Failed = failed, NotRun = notRun, Skipped = skipped }, summary);

	class TestableTestRunnerBase(ITest? test = null) :
		TestRunnerBase<TestRunnerBaseContext<ITest>, ITest>
	{
		public readonly ExceptionAggregator Aggregator = new();
		public readonly List<string> Invocations = [];
		public readonly SpyMessageBus MessageBus = new();
		public readonly ITest Test = test ?? Mocks.Test();
		public readonly CancellationTokenSource TokenSource = new();

		public IReadOnlyDictionary<string, TestAttachment>? GetAttachments__Result = null;
		public Action? GetAttachments__Lambda;

		protected override ValueTask<IReadOnlyDictionary<string, TestAttachment>?> GetAttachments(TestRunnerBaseContext<ITest> ctxt)
		{
			Invocations.Add("GetAttachments");

			GetAttachments__Lambda?.Invoke();

			return new(GetAttachments__Result);
		}

		public string GetTestOutput__Result = string.Empty;
		public Action? GetTestOutput__Lambda;

		protected override ValueTask<string> GetTestOutput(TestRunnerBaseContext<ITest> ctxt)
		{
			try
			{
				GetTestOutput__Lambda?.Invoke();

				return new(GetTestOutput__Result);
			}
			finally
			{
				Invocations.Add("GetTestOutput");
			}
		}

		public string[]? GetWarnings__Result;
		public Action? GetWarnings__Lambda;

		protected override ValueTask<string[]?> GetWarnings(TestRunnerBaseContext<ITest> ctxt)
		{
			try
			{
				GetWarnings__Lambda?.Invoke();

				return new(GetWarnings__Result);
			}
			finally
			{
				Invocations.Add("GetWarnings");
			}
		}

		public bool OnError__Result = true;

		protected override async ValueTask<bool> OnError(
			TestRunnerBaseContext<ITest> ctxt,
			Exception exception)
		{
			try
			{
				await base.OnError(ctxt, exception);

				return OnError__Result;
			}
			finally
			{
				Invocations.Add($"OnError(exception: {TypeName(exception)})");
			}
		}

		public Action? OnTestCleanupFailure__Lambda;
		public bool OnTestCleanupFailure__Result = true;

		protected override async ValueTask<bool> OnTestCleanupFailure(
			TestRunnerBaseContext<ITest> ctxt,
			Exception exception)
		{
			try
			{
				OnTestCleanupFailure__Lambda?.Invoke();

				await base.OnTestCleanupFailure(ctxt, exception);

				return OnTestCleanupFailure__Result;
			}
			finally
			{
				Invocations.Add($"OnTestCleanupFailure(exception: {TypeName(exception)})");
			}
		}

		public Action? OnTestFailed__Lambda;
		public bool OnTestFailed__Result = true;

		protected override async ValueTask<(bool Continue, TestResultState ResultState)> OnTestFailed(
			TestRunnerBaseContext<ITest> ctxt,
			Exception exception,
			decimal executionTime,
			string output,
			string[]? warnings)
		{
			try
			{
				OnTestFailed__Lambda?.Invoke();

				await base.OnTestFailed(ctxt, exception, executionTime, output, warnings);

				return (OnTestFailed__Result, TestResultState.FromException(0m, exception));
			}
			finally
			{
				Invocations.Add($"OnTestFailed(exception: {TypeName(exception)}, output: {ArgumentFormatter.Format(output)})");
			}
		}

		public Action? OnTestFinished__Lambda;
		public bool OnTestFinished__Result = true;

		protected override async ValueTask<bool> OnTestFinished(
			TestRunnerBaseContext<ITest> ctxt,
			decimal executionTime,
			string output,
			string[]? warnings,
			IReadOnlyDictionary<string, TestAttachment>? attachments)
		{
			var attachmentsDisplay =
				attachments is null
					? string.Empty
					: $", attachments: {ArgumentFormatter.Format(attachments)}";

			try
			{
				OnTestFinished__Lambda?.Invoke();

				await base.OnTestFinished(ctxt, executionTime, output, warnings, attachments);

				return OnTestFinished__Result;
			}
			finally
			{
				Invocations.Add($"OnTestFinished(output: {ArgumentFormatter.Format(output)}{attachmentsDisplay})");
			}
		}

		public Action? OnTestNotRun__Lambda;
		public bool OnTestNotRun__Result = true;

		protected override async ValueTask<(bool Continue, TestResultState ResultState)> OnTestNotRun(
			TestRunnerBaseContext<ITest> ctxt,
			string output,
			string[]? warnings)
		{
			try
			{
				OnTestNotRun__Lambda?.Invoke();

				await base.OnTestNotRun(ctxt, output, warnings);

				return (OnTestNotRun__Result, TestResultState.FromTestResult(TestData.TestNotRun()));
			}
			finally
			{
				Invocations.Add($"OnTestNotRun(output: {ArgumentFormatter.Format(output)})");
			}
		}

		public Action? OnTestPassed__Lambda;
		public bool OnTestPassed__Result = true;

		protected override async ValueTask<(bool Continue, TestResultState ResultState)> OnTestPassed(
			TestRunnerBaseContext<ITest> ctxt,
			decimal executionTime,
			string output,
			string[]? warnings)
		{
			try
			{
				OnTestPassed__Lambda?.Invoke();

				await base.OnTestPassed(ctxt, executionTime, output, warnings);

				return (OnTestPassed__Result, TestResultState.FromTestResult(TestData.TestPassed()));
			}
			finally
			{
				Invocations.Add($"OnTestPassed(output: {ArgumentFormatter.Format(output)})");
			}
		}

		public Action? OnTestSkipped__Lambda;
		public bool OnTestSkipped__Result = true;

		protected override async ValueTask<(bool Continue, TestResultState ResultState)> OnTestSkipped(
			TestRunnerBaseContext<ITest> ctxt,
			string skipReason,
			decimal executionTime,
			string output,
			string[]? warnings)
		{
			try
			{
				OnTestSkipped__Lambda?.Invoke();

				await base.OnTestSkipped(ctxt, skipReason, executionTime, output, warnings);

				return (OnTestSkipped__Result, TestResultState.FromTestResult(TestData.TestSkipped()));
			}
			finally
			{
				Invocations.Add($"OnTestSkipped(reason: {ArgumentFormatter.Format(skipReason)}, output: {ArgumentFormatter.Format(output)})");
			}
		}

		public Action? OnTestStarting__Lambda;
		public bool OnTestStarting__Result = true;

		protected override async ValueTask<bool> OnTestStarting(TestRunnerBaseContext<ITest> ctxt)
		{
			try
			{
				OnTestStarting__Lambda?.Invoke();

				await base.OnTestStarting(ctxt);

				return OnTestStarting__Result;
			}
			finally
			{
				Invocations.Add("OnTestStarting");
			}
		}

		public async ValueTask<RunSummary> Run(string? skipReason = null)
		{
			await using var ctxt = new TestRunnerBaseContext<ITest>(Test, MessageBus, skipReason, ExplicitOption.Off, Aggregator, TokenSource);
			await ctxt.InitializeAsync();

			return await Run(ctxt);
		}

		public Action? RunTest__Lambda;
		public TimeSpan RunTest__Result = TimeSpan.FromMilliseconds(42);

		protected override ValueTask<TimeSpan> RunTest(TestRunnerBaseContext<ITest> context)
		{
			try
			{
				RunTest__Lambda?.Invoke();

				return new(RunTest__Result);
			}
			finally
			{
				Invocations.Add("RunTest");
			}
		}

		public Action? ShouldTestRun__Lambda;
		public bool ShouldTestRun__Result = true;

		protected override bool ShouldTestRun(TestRunnerBaseContext<ITest> ctxt)
		{
			try
			{
				ShouldTestRun__Lambda?.Invoke();

				return ShouldTestRun__Result;
			}
			finally
			{
				Invocations.Add("ShouldTestRun");
			}
		}

		static string TypeName(object? value) =>
		value is null ? "null" : $"typeof({ArgumentFormatter.FormatTypeName(value.GetType())})";
	}
}
