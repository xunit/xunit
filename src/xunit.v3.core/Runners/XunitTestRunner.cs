using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test runner for xUnit.net v3 tests.
/// </summary>
public class XunitTestRunner : TestRunner<XunitTestRunnerContext, IXunitTest>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestRunner"/> class.
	/// </summary>
	protected XunitTestRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of the <see cref="XunitTestRunner"/>.
	/// </summary>
	public static XunitTestRunner Instance = new();

	/// <inheritdoc/>
	protected override async ValueTask<(object? Instance, SynchronizationContext? SyncContext, ExecutionContext? ExecutionContext)> CreateTestClassInstance(XunitTestRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var @class = ctxt.Test.TestMethod.TestClass.Class;

		// We allow for Func<T> when the argument is T, such that we should be able to get the value just before
		// invoking the test. So we need to do a transform on the arguments.
		object?[]? actualCtorArguments = null;

		if (ctxt.ConstructorArguments is not null)
		{
			var ctorParams =
				@class
					.GetConstructors()
					.Where(ci => !ci.IsStatic && ci.IsPublic)
					.Single()
					.GetParameters();

			actualCtorArguments = new object?[ctxt.ConstructorArguments.Length];

			for (var idx = 0; idx < ctxt.ConstructorArguments.Length; ++idx)
			{
				actualCtorArguments[idx] = ctxt.ConstructorArguments[idx];

				var ctorArgumentValueType = ctxt.ConstructorArguments[idx]?.GetType();
				if (ctorArgumentValueType is not null)
				{
					var ctorArgumentParamType = ctorParams[idx].ParameterType;
					if (ctorArgumentParamType != ctorArgumentValueType &&
						ctorArgumentValueType == typeof(Func<>).MakeGenericType(ctorArgumentParamType))
					{
						var invokeMethod = ctorArgumentValueType.GetMethod("Invoke", []);
						if (invokeMethod is not null)
							actualCtorArguments[idx] = invokeMethod.Invoke(ctxt.ConstructorArguments[idx], []);
					}
				}
			}
		}

		var instance = Activator.CreateInstance(@class, actualCtorArguments);
		if (instance is IAsyncLifetime asyncLifetime)
			await asyncLifetime.InitializeAsync();

		return (instance, SynchronizationContext.Current, ExecutionContext.Capture());
	}

	/// <inheritdoc/>
	protected override async ValueTask DisposeTestClassInstance(
		XunitTestRunnerContext ctxt,
		object testClassInstance)
	{
		Guard.ArgumentNotNull(ctxt);

		if (testClassInstance is IAsyncDisposable asyncDisposable)
			await asyncDisposable.DisposeAsync();
		else if (testClassInstance is IDisposable disposable)
			disposable.Dispose();
	}

	/// <inheritdoc/>
	protected override ValueTask<string> GetTestOutput(XunitTestRunnerContext ctxt) =>
		new(TestContext.Current.TestOutputHelper?.Output ?? string.Empty);

	/// <inheritdoc/>
	protected override ValueTask<TimeSpan> InvokeTestAsync(
		XunitTestRunnerContext ctxt,
		object? testClassInstance)
	{
		Guard.ArgumentNotNull(ctxt);

		return XunitTestInvoker.Instance.RunAsync(
			ctxt.Test,
			testClassInstance,
			ctxt.ExplicitOption,
			ctxt.MessageBus,
			// We don't clone the aggregator because invoker is an implementation detail in terms of
			// exceptions during execution; they should be bubbled up from the invoker to us
			ctxt.Aggregator,
			ctxt.CancellationTokenSource
		);
	}

	/// <inheritdoc/>
	protected override bool IsTestClassCreatable(XunitTestRunnerContext ctxt) =>
		!Guard.ArgumentNotNull(ctxt).Test.TestMethod.Method.IsStatic;

	/// <inheritdoc/>
	protected override bool IsTestClassDisposable(
		XunitTestRunnerContext ctxt,
		object testClassInstance) =>
			testClassInstance is IDisposable or IAsyncDisposable;

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestClassConstructionFinished(XunitTestRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return new(ctxt.MessageBus.QueueMessage(new TestClassConstructionFinished
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
		}));
	}

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestClassConstructionStarting(XunitTestRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return new(ctxt.MessageBus.QueueMessage(new TestClassConstructionStarting
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
		}));
	}

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestClassDisposeFinished(XunitTestRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return new(ctxt.MessageBus.QueueMessage(new TestClassDisposeFinished
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
		}));
	}

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestClassDisposeStarting(XunitTestRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return new(ctxt.MessageBus.QueueMessage(new TestClassDisposeStarting
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
		}));
	}

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestCleanupFailure(
		XunitTestRunnerContext ctxt,
		Exception exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var (types, messages, stackTraces, indices, _) = ExceptionUtility.ExtractMetadata(exception);

		return new(ctxt.MessageBus.QueueMessage(new TestCleanupFailure
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			ExceptionParentIndices = indices,
			ExceptionTypes = types,
			Messages = messages,
			StackTraces = stackTraces,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
		}));
	}

	/// <inheritdoc/>
	protected override ValueTask<(bool Continue, TestResultState ResultState)> OnTestFailed(
		XunitTestRunnerContext ctxt,
		Exception exception,
		decimal executionTime,
		string output)
	{
		Guard.ArgumentNotNull(ctxt);

		var (types, messages, stackTraces, indices, cause) = ExceptionUtility.ExtractMetadata(exception);

		var message = new TestFailed
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			Cause = cause,
			ExceptionParentIndices = indices,
			ExceptionTypes = types,
			ExecutionTime = executionTime,
			FinishTime = DateTimeOffset.UtcNow,
			Messages = messages,
			Output = output,
			StackTraces = stackTraces,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
			Warnings = TestContext.Current.Warnings?.ToArray(),
		};

		return new((ctxt.MessageBus.QueueMessage(message), TestResultState.FromTestResult(message)));
	}

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestFinished(
		XunitTestRunnerContext ctxt,
		decimal executionTime,
		string output)
	{
		Guard.ArgumentNotNull(ctxt);

		var result = ctxt.MessageBus.QueueMessage(new TestFinished
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			Attachments = TestContext.Current.Attachments ?? TestFinished.EmptyAttachments,
			ExecutionTime = executionTime,
			FinishTime = DateTimeOffset.UtcNow,
			Output = output,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
			Warnings = TestContext.Current.Warnings?.ToArray(),
		});

		(TestContext.Current.TestOutputHelper as TestOutputHelper)?.Uninitialize();

		return new(result);
	}

	/// <inheritdoc/>
	protected override ValueTask<(bool Continue, TestResultState ResultState)> OnTestNotRun(
		XunitTestRunnerContext ctxt,
		string output)
	{
		Guard.ArgumentNotNull(ctxt);

		var message = new TestNotRun
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			ExecutionTime = 0m,
			FinishTime = DateTimeOffset.UtcNow,
			Output = output,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
			Warnings = TestContext.Current.Warnings?.ToArray(),
		};

		return new((ctxt.MessageBus.QueueMessage(message), TestResultState.FromTestResult(message)));
	}

	/// <inheritdoc/>
	protected override ValueTask<(bool Continue, TestResultState ResultState)> OnTestPassed(
		XunitTestRunnerContext ctxt,
		decimal executionTime,
		string output)
	{
		Guard.ArgumentNotNull(ctxt);

		var message = new TestPassed
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			ExecutionTime = executionTime,
			FinishTime = DateTimeOffset.UtcNow,
			Output = output,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
			Warnings = TestContext.Current.Warnings?.ToArray(),
		};

		return new((ctxt.MessageBus.QueueMessage(message), TestResultState.FromTestResult(message)));
	}

	/// <inheritdoc/>
	protected override ValueTask<(bool Continue, TestResultState ResultState)> OnTestSkipped(
		XunitTestRunnerContext ctxt,
		string skipReason,
		decimal executionTime,
		string output)
	{
		Guard.ArgumentNotNull(ctxt);

		var message = new TestSkipped
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			ExecutionTime = executionTime,
			FinishTime = DateTimeOffset.UtcNow,
			Output = output,
			Reason = skipReason,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
			Warnings = TestContext.Current.Warnings?.ToArray(),
		};

		return new((ctxt.MessageBus.QueueMessage(message), TestResultState.FromTestResult(message)));
	}

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestStarting(XunitTestRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		(TestContext.Current.TestOutputHelper as TestOutputHelper)?.Initialize(ctxt.MessageBus, ctxt.Test);

		return new(ctxt.MessageBus.QueueMessage(new TestStarting
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			Explicit = ctxt.Test.Explicit,
			StartTime = DateTimeOffset.UtcNow,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestDisplayName = ctxt.Test.TestDisplayName,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
			Timeout = ctxt.Test.Timeout,
			Traits = ctxt.Test.Traits,
		}));
	}

	/// <inheritdoc/>
	protected override ValueTask PostInvoke(XunitTestRunnerContext ctxt) =>
		Guard.ArgumentNotNull(ctxt).RunAfterAttributes();

	/// <inheritdoc/>
	protected override ValueTask PreInvoke(XunitTestRunnerContext ctxt) =>
		Guard.ArgumentNotNull(ctxt).RunBeforeAttributes();

	/// <summary>
	/// Runs the test.
	/// </summary>
	/// <param name="test">The test that this invocation belongs to.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
	/// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="beforeAfterAttributes">The list of <see cref="IBeforeAfterTestAttribute"/>s for this test.</param>
	/// <returns>Returns summary information about the test that was run.</returns>
	public async ValueTask<RunSummary> RunAsync(
		IXunitTest test,
		IMessageBus messageBus,
		object?[] constructorArguments,
		string? skipReason,
		ExplicitOption explicitOption,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		IReadOnlyCollection<IBeforeAfterTestAttribute> beforeAfterAttributes)
	{
		await using var ctxt = new XunitTestRunnerContext(test, messageBus, skipReason, explicitOption, aggregator, cancellationTokenSource, beforeAfterAttributes, constructorArguments);
		await ctxt.InitializeAsync();

		return await RunAsync(ctxt);
	}

	/// <inheritdoc/>
	protected override void SetTestContext(
		XunitTestRunnerContext ctxt,
		TestEngineStatus testStatus,
		TestResultState? testState = null,
		object? testClassInstance = null)
	{
		Guard.ArgumentNotNull(ctxt);

		TestContext.SetForTest(
			ctxt.Test,
			testStatus,
			ctxt.CancellationTokenSource.Token,
			testState,
			testStatus == TestEngineStatus.Initializing ? new TestOutputHelper() : TestContext.Current.TestOutputHelper,
			testClassInstance: testClassInstance
		);
	}

	/// <inheritdoc/>
	protected override bool ShouldTestRun(XunitTestRunnerContext ctxt) =>
		Guard.ArgumentNotNull(ctxt).ExplicitOption switch
		{
			ExplicitOption.Only => ctxt.Test.Explicit,
			ExplicitOption.Off => !ctxt.Test.Explicit,
			_ => true,
		};
}
