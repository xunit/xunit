using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestRunnerTests
{
	public class Invocations
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public static async ValueTask Passed(bool cancel)
		{
			var runner = new TestableTestRunner
			{
				CreateTestClassInstance__Result = new object(),  // Make sure to pass an instance so we can see the disposal flow as well as the creation flow
				GetTestOutput__Result = "the output",
				OnTestPassed__Result = !cancel,
			};

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary);
			Assert.Equal(cancel, runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"IsTestClassCreatable",
				"OnTestClassConstructionStarting",
				"CreateTestClassInstance",
				"OnTestClassConstructionFinished",
				"InvokeTestAsync(testClassInstance: typeof(object))",
				"IsTestClassDisposable",
				"OnTestClassDisposeStarting",
				"DisposeTestClassInstance",
				"OnTestClassDisposeFinished",
				"GetTestOutput",
				"OnTestPassed(output: \"the output\")",
				"OnTestFinished(output: \"the output\")",
				// OnTestCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public static async ValueTask Failed(bool cancel)
		{
			var runner = new TestableTestRunner
			{
				GetTestOutput__Result = "the output",
				InvokeTestAsync__Lambda = () => Assert.True(false),
				IsTestClassCreatable__Result = false,  // Turning off creation just to make sure we don't call things we don't need to
				OnTestFailed__Result = !cancel,
			};

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, failed: 1);
			Assert.Equal(cancel, runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"IsTestClassCreatable",
				// OnTestClassConstructionStarting
				// CreateTestClassInstance
				// OnTestClassConstructionFinished
				"InvokeTestAsync(testClassInstance: null)",
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"GetTestOutput",
				"OnTestFailed(exception: typeof(TrueException), output: \"the output\")",
				"OnTestFinished(output: \"the output\")",
				// OnTestCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public static async ValueTask Skipped_Static(bool cancel)
		{
			var runner = new TestableTestRunner
			{
				OnTestSkipped__Result = !cancel,
				RunAsync__SkipReason = "Don't run me",
			};

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, skipped: 1);
			Assert.Equal(cancel, runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				// IsTestClassCreatable
				// OnTestClassConstructionStarting
				// CreateTestClassInstance
				// OnTestClassConstructionFinished
				// InvokeTestAsync
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"GetTestOutput",
				"OnTestSkipped(reason: \"Don't run me\", output: \"\")",
				"OnTestFinished(output: \"\")",
				// OnTestCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public static async ValueTask Skipped_Dynamic(bool cancel)
		{
			var runner = new TestableTestRunner
			{
				GetTestOutput__Result = "the output",
				InvokeTestAsync__Lambda = () => throw SkipException.ForSkip("This isn't a good time"),
				OnTestSkipped__Result = !cancel,
			};

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, skipped: 1);
			Assert.Equal(cancel, runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"IsTestClassCreatable",
				"OnTestClassConstructionStarting",
				"CreateTestClassInstance",
				"OnTestClassConstructionFinished",
				"InvokeTestAsync(testClassInstance: null)",
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"GetTestOutput",
				"OnTestSkipped(reason: \"This isn't a good time\", output: \"the output\")",
				"OnTestFinished(output: \"the output\")",
				// OnTestCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public static async ValueTask NotRun(bool cancel)
		{
			var runner = new TestableTestRunner
			{
				OnTestNotRun__Result = !cancel,
				ShouldTestRun__Result = false,
			};

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, notRun: 1);
			Assert.Equal(cancel, runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				// IsTestClassCreatable
				// OnTestClassConstructionStarting
				// CreateTestClassInstance
				// OnTestClassConstructionFinished
				// InvokeTestAsync
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"GetTestOutput",
				"OnTestNotRun(output: \"\")",
				"OnTestFinished(output: \"\")",
				// OnTestCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}
	}

	public class HandlerExceptions
	{
		[Fact]
		public static async ValueTask CreateTestClassInstance()
		{
			var runner = new TestableTestRunner { CreateTestClassInstance__Lambda = () => throw new DivideByZeroException() };

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, failed: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"IsTestClassCreatable",
				"OnTestClassConstructionStarting",
				"CreateTestClassInstance",
				"OnTestClassConstructionFinished",
				// InvokeTestAsync
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"GetTestOutput",
				"OnTestFailed(exception: typeof(DivideByZeroException), output: \"\")",
				"OnTestFinished(output: \"\")",
				// OnTestCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask DisposeTestClassInstance()
		{
			var runner = new TestableTestRunner
			{
				CreateTestClassInstance__Result = new object(),
				DisposeTestClassInstance__Lambda = () => throw new DivideByZeroException(),
			};

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, failed: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"IsTestClassCreatable",
				"OnTestClassConstructionStarting",
				"CreateTestClassInstance",
				"OnTestClassConstructionFinished",
				"InvokeTestAsync(testClassInstance: typeof(object))",
				"IsTestClassDisposable",
				"OnTestClassDisposeStarting",
				"DisposeTestClassInstance",
				"OnTestClassDisposeFinished",
				"GetTestOutput",
				"OnTestFailed(exception: typeof(DivideByZeroException), output: \"\")",
				"OnTestFinished(output: \"\")",
				// OnTestCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask GetTestOutput()
		{
			var runner = new TestableTestRunner { GetTestOutput__Lambda = () => throw new DivideByZeroException() };

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, failed: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"IsTestClassCreatable",
				"OnTestClassConstructionStarting",
				"CreateTestClassInstance",
				"OnTestClassConstructionFinished",
				"InvokeTestAsync(testClassInstance: null)",
				"GetTestOutput",
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"OnTestFailed(exception: typeof(DivideByZeroException), output: \"\")",
				"OnTestFinished(output: \"\")",
				// OnTestCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask IsTestClassCreatable()
		{
			var runner = new TestableTestRunner { IsTestClassCreatable__Lambda = () => throw new DivideByZeroException() };

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, failed: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"IsTestClassCreatable",
				// OnTestClassConstructionStarting
				// CreateTestClassInstance
				// OnTestClassConstructionFinished
				// InvokeTestAsync
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"GetTestOutput",
				"OnTestFailed(exception: typeof(DivideByZeroException), output: \"\")",
				"OnTestFinished(output: \"\")",
				// OnTestCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask IsTestClassDisposable()
		{
			var runner = new TestableTestRunner
			{
				CreateTestClassInstance__Result = new object(),
				IsTestClassDisposable__Lambda = () => throw new DivideByZeroException(),
			};

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, failed: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"IsTestClassCreatable",
				"OnTestClassConstructionStarting",
				"CreateTestClassInstance",
				"OnTestClassConstructionFinished",
				"InvokeTestAsync(testClassInstance: typeof(object))",
				"IsTestClassDisposable",
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"GetTestOutput",
				"OnTestFailed(exception: typeof(DivideByZeroException), output: \"\")",
				"OnTestFinished(output: \"\")",
				// OnTestCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestClassConstructionFinished()
		{
			var runner = new TestableTestRunner { OnTestClassConstructionFinished__Lambda = () => throw new DivideByZeroException() };

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, failed: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"IsTestClassCreatable",
				"OnTestClassConstructionStarting",
				"CreateTestClassInstance",
				"OnTestClassConstructionFinished",
				// InvokeTestAsync
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"GetTestOutput",
				"OnTestFailed(exception: typeof(DivideByZeroException), output: \"\")",
				"OnTestFinished(output: \"\")",
				// OnTestCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestClassConstructionStarting()
		{
			var runner = new TestableTestRunner { OnTestClassConstructionStarting__Lambda = () => throw new DivideByZeroException() };

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, failed: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"IsTestClassCreatable",
				"OnTestClassConstructionStarting",
				// CreateTestClassInstance
				// OnTestClassConstructionFinished
				// InvokeTestAsync
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"GetTestOutput",
				"OnTestFailed(exception: typeof(DivideByZeroException), output: \"\")",
				"OnTestFinished(output: \"\")",
				// OnTestCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestClassDisposeFinished()
		{
			var runner = new TestableTestRunner
			{
				CreateTestClassInstance__Result = new object(),
				OnTestClassDisposeFinished__Lambda = () => throw new DivideByZeroException(),
			};

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, failed: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"IsTestClassCreatable",
				"OnTestClassConstructionStarting",
				"CreateTestClassInstance",
				"OnTestClassConstructionFinished",
				"InvokeTestAsync(testClassInstance: typeof(object))",
				"IsTestClassDisposable",
				"OnTestClassDisposeStarting",
				"DisposeTestClassInstance",
				"OnTestClassDisposeFinished",
				"GetTestOutput",
				"OnTestFailed(exception: typeof(DivideByZeroException), output: \"\")",
				"OnTestFinished(output: \"\")",
				// OnTestCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestClassDisposeStarting()
		{
			var runner = new TestableTestRunner
			{
				CreateTestClassInstance__Result = new object(),
				OnTestClassDisposeStarting__Lambda = () => throw new DivideByZeroException(),
			};

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, failed: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"IsTestClassCreatable",
				"OnTestClassConstructionStarting",
				"CreateTestClassInstance",
				"OnTestClassConstructionFinished",
				"InvokeTestAsync(testClassInstance: typeof(object))",
				"IsTestClassDisposable",
				"OnTestClassDisposeStarting",
				"DisposeTestClassInstance",
				"OnTestClassDisposeFinished",
				"GetTestOutput",
				"OnTestFailed(exception: typeof(DivideByZeroException), output: \"\")",
				"OnTestFinished(output: \"\")",
				// OnTestCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestCleanupFailure()
		{
			var runner = new TestableTestRunner
			{
				// Need to throw in OnTestFinished to get OnTestCleanupFailure to trigger
				OnTestFinished__Lambda = () => throw new ArgumentException(),
				OnTestCleanupFailure__Lambda = () => throw new DivideByZeroException(),
			};

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"IsTestClassCreatable",
				"OnTestClassConstructionStarting",
				"CreateTestClassInstance",
				"OnTestClassConstructionFinished",
				"InvokeTestAsync(testClassInstance: null)",
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"GetTestOutput",
				"OnTestPassed(output: \"\")",
				"OnTestFinished(output: \"\")",
				"OnTestCleanupFailure(exception: typeof(ArgumentException))",
			}, runner.Invocations);
			var message = Assert.Single(runner.MessageBus.Messages);
			var errorMessage = Assert.IsAssignableFrom<IErrorMessage>(message);
			Assert.Equal(new[] { -1 }, errorMessage.ExceptionParentIndices);
			Assert.Equal(new[] { "System.DivideByZeroException" }, errorMessage.ExceptionTypes);
			Assert.Equal(new[] { "Attempted to divide by zero." }, errorMessage.Messages);
			Assert.NotEmpty(errorMessage.StackTraces.Single()!);
		}

		[Fact]
		public static async ValueTask OnTestFailed()
		{
			var runner = new TestableTestRunner
			{
				InvokeTestAsync__Lambda = () => Assert.True(false),
				OnTestFailed__Lambda = () => throw new DivideByZeroException(),
			};

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, failed: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"IsTestClassCreatable",
				"OnTestClassConstructionStarting",
				"CreateTestClassInstance",
				"OnTestClassConstructionFinished",
				"InvokeTestAsync(testClassInstance: null)",
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"GetTestOutput",
				"OnTestFailed(exception: typeof(TrueException), output: \"\")",
				"OnTestFinished(output: \"\")",
				"OnTestCleanupFailure(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestFinished()
		{
			var runner = new TestableTestRunner { OnTestFinished__Lambda = () => throw new DivideByZeroException() };

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"IsTestClassCreatable",
				"OnTestClassConstructionStarting",
				"CreateTestClassInstance",
				"OnTestClassConstructionFinished",
				"InvokeTestAsync(testClassInstance: null)",
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"GetTestOutput",
				"OnTestPassed(output: \"\")",
				"OnTestFinished(output: \"\")",
				"OnTestCleanupFailure(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestNotRun()
		{
			var runner = new TestableTestRunner
			{
				OnTestNotRun__Lambda = () => throw new DivideByZeroException(),
				ShouldTestRun__Result = false,
			};

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, notRun: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				// IsTestClassCreatable
				// OnTestClassConstructionStarting
				// CreateTestClassInstance
				// OnTestClassConstructionFinished
				// InvokeTestAsync
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"GetTestOutput",
				"OnTestNotRun(output: \"\")",
				"OnTestFinished(output: \"\")",
				"OnTestCleanupFailure(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestPassed()
		{
			var runner = new TestableTestRunner { OnTestPassed__Lambda = () => throw new DivideByZeroException() };

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"IsTestClassCreatable",
				"OnTestClassConstructionStarting",
				"CreateTestClassInstance",
				"OnTestClassConstructionFinished",
				"InvokeTestAsync(testClassInstance: null)",
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"GetTestOutput",
				"OnTestPassed(output: \"\")",
				"OnTestFinished(output: \"\")",
				"OnTestCleanupFailure(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestSkipped()
		{
			var runner = new TestableTestRunner
			{
				OnTestSkipped__Lambda = () => throw new DivideByZeroException(),
				RunAsync__SkipReason = "Don't run me",
			};

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, skipped: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				// IsTestClassCreatable
				// OnTestClassConstructionStarting
				// CreateTestClassInstance
				// OnTestClassConstructionFinished
				// InvokeTestAsync
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"GetTestOutput",
				"OnTestSkipped(reason: \"Don't run me\", output: \"\")",
				"OnTestFinished(output: \"\")",
				"OnTestCleanupFailure(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestStarting()
		{
			var runner = new TestableTestRunner { OnTestStarting__Lambda = () => throw new DivideByZeroException() };

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, total: 1, failed: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				// ShouldTestRun
				// IsTestClassCreatable
				// OnTestClassConstructionStarting
				// CreateTestClassInstance
				// OnTestClassConstructionFinished
				// InvokeTestAsync
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"GetTestOutput",
				"OnTestFailed(exception: typeof(DivideByZeroException), output: \"\")",
				"OnTestFinished(output: \"\")",
				// OnTestCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask ShouldTestRun()
		{
			var runner = new TestableTestRunner { ShouldTestRun__Lambda = () => throw new DivideByZeroException() };

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, failed: 1);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				// IsTestClassCreatable
				// OnTestClassConstructionStarting
				// CreateTestClassInstance
				// OnTestClassConstructionFinished
				// InvokeTestAsync
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"GetTestOutput",
				"OnTestFailed(exception: typeof(DivideByZeroException), output: \"\")",
				"OnTestFinished(output: \"\")",
				// OnTestCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

	}

	// We test the standard handlers (OnTestPassed, OnTestFailed, etc.) via theory in Invocations above
	// so these tests are the ancillary cancellable handlers (starting, finished, and cleanup failure).
	public class Cancellation
	{
		[Fact]
		public static async ValueTask OnTestCleanupFailure()
		{
			var runner = new TestableTestRunner
			{
				// Need to throw in OnTestFinished to get OnTestCleanupFailure to trigger
				OnTestCleanupFailure__Result = false,
				OnTestFinished__Lambda = () => throw new ArgumentException(),
			};

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, total: 1);
			Assert.True(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"IsTestClassCreatable",
				"OnTestClassConstructionStarting",
				"CreateTestClassInstance",
				"OnTestClassConstructionFinished",
				"InvokeTestAsync(testClassInstance: null)",
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"GetTestOutput",
				"OnTestPassed(output: \"\")",
				"OnTestFinished(output: \"\")",
				"OnTestCleanupFailure(exception: typeof(ArgumentException))",
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestFinished()
		{
			var runner = new TestableTestRunner { OnTestFinished__Result = false };

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary);
			Assert.True(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				"ShouldTestRun",
				"IsTestClassCreatable",
				"OnTestClassConstructionStarting",
				"CreateTestClassInstance",
				"OnTestClassConstructionFinished",
				"InvokeTestAsync(testClassInstance: null)",
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				"GetTestOutput",
				"OnTestPassed(output: \"\")",
				"OnTestFinished(output: \"\")",
				// OnTestCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestStarting()
		{
			var runner = new TestableTestRunner { OnTestStarting__Result = false };

			var summary = await runner.RunAsync();

			VerifyRunSummary(summary, total: 0);
			Assert.True(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestStarting",
				// ShouldTestRun
				// IsTestClassCreatable
				// OnTestClassConstructionStarting
				// CreateTestClassInstance
				// OnTestClassConstructionFinished
				// InvokeTestAsync
				// IsTestClassDisposable
				// OnTestClassDisposeStarting
				// DisposeTestClassInstance
				// OnTestClassDisposeFinished
				// GetTestOutput
				// OnTestXxx
				"OnTestFinished(output: \"\")",
				// OnTestCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}
	}

	static void VerifyRunSummary(
		RunSummary summary,
		int total = 1,
		int failed = 0,
		int notRun = 0,
		int skipped = 0) =>
			Assert.Equivalent(new { Total = total, Failed = failed, NotRun = notRun, Skipped = skipped }, summary);

	class TestableTestRunner(ITest? test = null) :
		TestRunner<TestRunnerContext<ITest>, ITest>
	{
		public readonly ExceptionAggregator Aggregator = new();
		public readonly List<string> Invocations = [];
		public readonly SpyMessageBus MessageBus = new();
		public readonly ITest Test = test ?? Mocks.Test();
		public readonly CancellationTokenSource TokenSource = new();

		public Action? CreateTestClassInstance__Lambda;
		public object? CreateTestClassInstance__Result;

		protected override ValueTask<object?> CreateTestClassInstance(TestRunnerContext<ITest> ctxt)
		{
			Invocations.Add("CreateTestClassInstance");

			CreateTestClassInstance__Lambda?.Invoke();

			return new(CreateTestClassInstance__Result);
		}

		public Action? DisposeTestClassInstance__Lambda;

		protected override ValueTask DisposeTestClassInstance(
			TestRunnerContext<ITest> ctxt,
			object testClassInstance)
		{
			Invocations.Add("DisposeTestClassInstance");

			DisposeTestClassInstance__Lambda?.Invoke();

			return default;
		}

		public string GetTestOutput__Result = string.Empty;
		public Action? GetTestOutput__Lambda;

		protected override ValueTask<string> GetTestOutput(TestRunnerContext<ITest> ctxt)
		{
			Invocations.Add("GetTestOutput");

			GetTestOutput__Lambda?.Invoke();

			return new(GetTestOutput__Result);
		}

		public Action? InvokeTestAsync__Lambda;
		public TimeSpan InvokeTestAsync__Result = TimeSpan.Zero;

		protected override ValueTask<TimeSpan> InvokeTestAsync(
			TestRunnerContext<ITest> ctxt,
			object? testClassInstance)
		{
			Assert.Same(CreateTestClassInstance__Result, testClassInstance);

			Invocations.Add($"InvokeTestAsync(testClassInstance: {TypeName(testClassInstance)})");

			InvokeTestAsync__Lambda?.Invoke();

			return new(InvokeTestAsync__Result);
		}

		public Action? IsTestClassCreatable__Lambda;
		public bool IsTestClassCreatable__Result = true;

		protected override bool IsTestClassCreatable(TestRunnerContext<ITest> ctxt)
		{
			Invocations.Add("IsTestClassCreatable");

			IsTestClassCreatable__Lambda?.Invoke();

			return IsTestClassCreatable__Result;
		}

		public Action? IsTestClassDisposable__Lambda;
		public bool IsTestClassDisposable__Result = true;

		protected override bool IsTestClassDisposable(
			TestRunnerContext<ITest> ctxt,
			object testClassInstance)
		{
			Invocations.Add("IsTestClassDisposable");

			IsTestClassDisposable__Lambda?.Invoke();

			return IsTestClassDisposable__Result;
		}

		public Action? OnTestClassConstructionFinished__Lambda;
		public bool OnTestClassConstructionFinished__Result = true;

		protected override ValueTask<bool> OnTestClassConstructionFinished(TestRunnerContext<ITest> ctxt)
		{
			Invocations.Add("OnTestClassConstructionFinished");

			OnTestClassConstructionFinished__Lambda?.Invoke();

			return new(OnTestClassConstructionFinished__Result);
		}

		public Action? OnTestClassConstructionStarting__Lambda;
		public bool OnTestClassConstructionStarting__Result = true;

		protected override ValueTask<bool> OnTestClassConstructionStarting(TestRunnerContext<ITest> ctxt)
		{
			Invocations.Add("OnTestClassConstructionStarting");

			OnTestClassConstructionStarting__Lambda?.Invoke();

			return new(OnTestClassConstructionStarting__Result);
		}

		public Action? OnTestClassDisposeFinished__Lambda;
		public bool OnTestClassDisposeFinished__Result = true;

		protected override ValueTask<bool> OnTestClassDisposeFinished(TestRunnerContext<ITest> ctxt)
		{
			Invocations.Add("OnTestClassDisposeFinished");

			OnTestClassDisposeFinished__Lambda?.Invoke();

			return new(OnTestClassDisposeFinished__Result);
		}

		public Action? OnTestClassDisposeStarting__Lambda;
		public bool OnTestClassDisposeStarting__Result = true;

		protected override ValueTask<bool> OnTestClassDisposeStarting(TestRunnerContext<ITest> ctxt)
		{
			Invocations.Add("OnTestClassDisposeStarting");

			OnTestClassDisposeStarting__Lambda?.Invoke();

			return new(OnTestClassDisposeStarting__Result);
		}

		public Action? OnTestCleanupFailure__Lambda;
		public bool OnTestCleanupFailure__Result = true;

		protected override ValueTask<bool> OnTestCleanupFailure(
			TestRunnerContext<ITest> ctxt,
			Exception exception)
		{
			Invocations.Add($"OnTestCleanupFailure(exception: {TypeName(exception)})");

			OnTestCleanupFailure__Lambda?.Invoke();

			return new(OnTestCleanupFailure__Result);
		}

		public Action? OnTestFailed__Lambda;
		public bool OnTestFailed__Result = true;

		protected override ValueTask<(bool Continue, TestResultState ResultState)> OnTestFailed(
			TestRunnerContext<ITest> ctxt,
			Exception exception,
			decimal executionTime,
			string output)
		{
			Invocations.Add($"OnTestFailed(exception: {TypeName(exception)}, output: {ArgumentFormatter.Format(output)})");

			OnTestFailed__Lambda?.Invoke();

			return new((OnTestFailed__Result, TestResultState.FromException(0m, exception)));
		}

		public Action? OnTestFinished__Lambda;
		public bool OnTestFinished__Result = true;

		protected override ValueTask<bool> OnTestFinished(
			TestRunnerContext<ITest> ctxt,
			decimal executionTime,
			string output)
		{
			Invocations.Add($"OnTestFinished(output: {ArgumentFormatter.Format(output)})");

			OnTestFinished__Lambda?.Invoke();

			return new(OnTestFinished__Result);
		}

		public Action? OnTestNotRun__Lambda;
		public bool OnTestNotRun__Result = true;

		protected override ValueTask<(bool Continue, TestResultState ResultState)> OnTestNotRun(
			TestRunnerContext<ITest> ctxt,
			string output)
		{
			Invocations.Add($"OnTestNotRun(output: {ArgumentFormatter.Format(output)})");

			OnTestNotRun__Lambda?.Invoke();

			return new((OnTestNotRun__Result, TestResultState.FromTestResult(TestData.TestNotRun())));
		}

		public Action? OnTestPassed__Lambda;
		public bool OnTestPassed__Result = true;

		protected override ValueTask<(bool Continue, TestResultState ResultState)> OnTestPassed(
			TestRunnerContext<ITest> ctxt,
			decimal executionTime,
			string output)
		{
			Invocations.Add($"OnTestPassed(output: {ArgumentFormatter.Format(output)})");

			OnTestPassed__Lambda?.Invoke();

			return new((OnTestPassed__Result, TestResultState.FromTestResult(TestData.TestPassed())));
		}

		public Action? OnTestSkipped__Lambda;
		public bool OnTestSkipped__Result = true;

		protected override ValueTask<(bool Continue, TestResultState ResultState)> OnTestSkipped(
			TestRunnerContext<ITest> ctxt,
			string skipReason,
			decimal executionTime,
			string output)
		{
			Invocations.Add($"OnTestSkipped(reason: {ArgumentFormatter.Format(skipReason)}, output: {ArgumentFormatter.Format(output)})");

			OnTestSkipped__Lambda?.Invoke();

			return new((OnTestSkipped__Result, TestResultState.FromTestResult(TestData.TestSkipped())));
		}

		public Action? OnTestStarting__Lambda;
		public bool OnTestStarting__Result = true;

		protected override ValueTask<bool> OnTestStarting(TestRunnerContext<ITest> ctxt)
		{
			Invocations.Add("OnTestStarting");

			OnTestStarting__Lambda?.Invoke();

			return new(OnTestStarting__Result);
		}

		public string? RunAsync__SkipReason = null;

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var ctxt = new TestRunnerContext<ITest>(Test, MessageBus, RunAsync__SkipReason, ExplicitOption.Off, Aggregator, TokenSource);
			await ctxt.InitializeAsync();

			return await RunAsync(ctxt);
		}

		public Action? ShouldTestRun__Lambda;
		public bool ShouldTestRun__Result = true;

		protected override bool ShouldTestRun(TestRunnerContext<ITest> ctxt)
		{
			Invocations.Add("ShouldTestRun");

			ShouldTestRun__Lambda?.Invoke();

			return ShouldTestRun__Result;
		}

		static string TypeName(object? value) =>
			value is null ? "null" : $"typeof({ArgumentFormatter.FormatTypeName(value.GetType())})";
	}
}
