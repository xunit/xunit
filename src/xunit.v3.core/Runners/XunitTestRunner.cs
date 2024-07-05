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
	protected override async ValueTask<object?> CreateTestClassInstance(XunitTestRunnerContext ctxt)
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

		return instance;
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
			ctxt.TestMethodArguments,
			ctxt.ExplicitOption,
			ctxt.MessageBus,
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
			testClassInstance is IDisposable || testClassInstance is IAsyncDisposable;

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestClassConstructionFinished(XunitTestRunnerContext ctxt) =>
		new(ReportMessage(ctxt, new TestClassConstructionFinished()).Continue);

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestClassConstructionStarting(XunitTestRunnerContext ctxt) =>
		new(ReportMessage(ctxt, new TestClassConstructionStarting()).Continue);

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestClassDisposeFinished(XunitTestRunnerContext ctxt) =>
		new(ReportMessage(ctxt, new TestClassDisposeFinished()).Continue);

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestClassDisposeStarting(XunitTestRunnerContext ctxt) =>
		new(ReportMessage(ctxt, new TestClassDisposeStarting()).Continue);

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestCleanupFailure(
		XunitTestRunnerContext ctxt,
		Exception exception) =>
			new(ReportMessage(ctxt, new TestCleanupFailure(), exception: exception).Continue);

	/// <inheritdoc/>
	protected override ValueTask<(bool Continue, TestResultState ResultState)> OnTestFailed(
		XunitTestRunnerContext ctxt,
		Exception exception,
		decimal executionTime,
		string output) =>
			new(ReportMessage(ctxt, new TestFailed(), executionTime, output, exception));

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestFinished(
		XunitTestRunnerContext ctxt,
		decimal executionTime,
		string output)
	{
		Guard.ArgumentNotNull(ctxt);

		var result = ReportMessage(ctxt, new TestFinished(), executionTime, output).Continue;

		(TestContext.Current.TestOutputHelper as TestOutputHelper)?.Uninitialize();

		return new(result);
	}

	/// <inheritdoc/>
	protected override ValueTask<(bool Continue, TestResultState ResultState)> OnTestNotRun(
		XunitTestRunnerContext ctxt,
		string output) =>
			new(ReportMessage(ctxt, new TestNotRun(), output: output));

	/// <inheritdoc/>
	protected override ValueTask<(bool Continue, TestResultState ResultState)> OnTestPassed(
		XunitTestRunnerContext ctxt,
		decimal executionTime,
		string output) =>
			new(ReportMessage(ctxt, new TestPassed(), executionTime, output));

	/// <inheritdoc/>
	protected override ValueTask<(bool Continue, TestResultState ResultState)> OnTestSkipped(
		XunitTestRunnerContext ctxt,
		string skipReason,
		decimal executionTime,
		string output) =>
			new(ReportMessage(ctxt, new TestSkipped { Reason = skipReason }, executionTime, output));

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestStarting(XunitTestRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		(TestContext.Current.TestOutputHelper as TestOutputHelper)?.Initialize(ctxt.MessageBus, ctxt.Test);

		var result = ReportMessage(ctxt, new TestStarting
		{
			Explicit = ctxt.Test.Explicit,
			TestDisplayName = ctxt.Test.TestDisplayName,
			Timeout = ctxt.Test.Timeout,
			Traits = ctxt.Test.Traits,
		}).Continue;

		return new(result);
	}

	/// <inheritdoc/>
	protected override ValueTask PostInvoke(XunitTestRunnerContext ctxt) =>
		Guard.ArgumentNotNull(ctxt).RunAfterAttributes();

	/// <inheritdoc/>
	protected override ValueTask PreInvoke(XunitTestRunnerContext ctxt) =>
		Guard.ArgumentNotNull(ctxt).RunBeforeAttributes();

	static (bool Continue, TestResultState ResultState) ReportMessage(
		XunitTestRunnerContext ctxt,
		TestMessage message,
		decimal executionTime = 0m,
		string output = "",
		Exception? exception = null)
	{
		Guard.ArgumentNotNull(ctxt);

		message.AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID;
		message.TestCaseUniqueID = ctxt.Test.TestCase.UniqueID;
		message.TestClassUniqueID = ctxt.Test.TestCase.TestClass.UniqueID;
		message.TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID;
		message.TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID;
		message.TestUniqueID = ctxt.Test.UniqueID;

		if (exception is not null && message is IWritableErrorMetadata errorMessage)
		{
			var errorMetadata = ExceptionUtility.ExtractMetadata(exception);

			errorMessage.ExceptionParentIndices = errorMetadata.ExceptionParentIndices;
			errorMessage.ExceptionTypes = errorMetadata.ExceptionTypes;
			errorMessage.Messages = errorMetadata.Messages;
			errorMessage.StackTraces = errorMetadata.StackTraces;

			if (message is TestFailed testFailed)
				testFailed.Cause = errorMetadata.Cause;
		}

		var testResultState = TestResultState.ForPassed(executionTime);

		if (message is TestResultMessage resultMessage)
		{
			resultMessage.ExecutionTime = executionTime;
			resultMessage.Output = output;
			resultMessage.Warnings = TestContext.Current.Warnings?.ToArray();

			// This needs to be absolutely last in this method, because it depends on the result
			// message being completely filled out.
			if (message is not TestFinished)
				testResultState = TestResultState.FromTestResult(resultMessage);
		}

		return (ctxt.MessageBus.QueueMessage(message), testResultState);
	}

	/// <summary>
	/// Runs the test.
	/// </summary>
	/// <param name="test">The test that this invocation belongs to.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
	/// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
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
		object?[] testMethodArguments,
		string? skipReason,
		ExplicitOption explicitOption,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		IReadOnlyCollection<IBeforeAfterTestAttribute> beforeAfterAttributes)
	{
		await using var ctxt = new XunitTestRunnerContext(test, messageBus, skipReason, explicitOption, aggregator, cancellationTokenSource, beforeAfterAttributes, constructorArguments, testMethodArguments);
		await ctxt.InitializeAsync();

		return await RunAsync(ctxt);
	}

	/// <inheritdoc/>
	protected override void SetTestContext(
		XunitTestRunnerContext ctxt,
		TestEngineStatus testStatus,
		TestResultState? testState = null)
	{
		Guard.ArgumentNotNull(ctxt);

		TestContext.SetForTest(
			ctxt.Test,
			testStatus,
			ctxt.CancellationTokenSource.Token,
			testState,
			testStatus == TestEngineStatus.Initializing ? new TestOutputHelper() : TestContext.Current.TestOutputHelper
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
