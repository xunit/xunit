using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base class that provides default behavior when running tests in a test class. It groups the tests
/// by test method, and then runs the individual test methods.
/// </summary>
/// <typeparam name="TContext">The context type used by the runner</typeparam>
/// <typeparam name="TTestClass">The type of the test class used by the test framework.
/// Must derive from <see cref="ITestClass"/>.</typeparam>
/// <typeparam name="TTestMethod">The type of the test method used by the test framework.
/// Must derive from <see cref="ITestMethod"/>.</typeparam>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ITestCase"/>.</typeparam>
public abstract class TestClassRunner<TContext, TTestClass, TTestMethod, TTestCase>
	where TContext : TestClassRunnerContext<TTestClass, TTestCase>
	where TTestClass : class, ITestClass
	where TTestMethod : class, ITestMethod
	where TTestCase : class, ITestCase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestClassRunner{TContext, TTestClass, TTestMethod, TTestCase}"/> class.
	/// </summary>
	protected TestClassRunner()
	{ }

	/// <summary>
	/// Creates the arguments for the test class constructor. By default just returns an empty
	/// set of arguments. Override to find the arguments for the constructor. Arguments without
	/// matching values may be returned as <see cref="Missing.Value"/>, as the <see cref="ITypeActivator"/>
	/// is responsible for determining how to resolve unknown arguments.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Initializing"/> and any exceptions thrown will
	/// contribute to test class failure (and will prevent the test class from running)
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <returns>The test class constructor arguments.</returns>
	protected virtual ValueTask<object?[]> CreateTestClassConstructorArguments(TContext ctxt) =>
		new([]);

	/// <summary>
	/// Fails the tests from a test method due to an exception.
	/// </summary>
	/// <remarks>
	/// By default, using <see cref="XunitRunnerHelper"/> to fail the test cases.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="testMethod">The test method that contains the test cases. May be <see langword="null"/> for test cases that do not
	/// support classes and methods.</param>
	/// <param name="testCases">The test cases to be failed.</param>
	/// <param name="constructorArguments">The constructor arguments that would have been used to create the test class.</param>
	/// <param name="exception">The exception that was caused during startup.</param>
	/// <returns>Returns summary information about the tests that were failed.</returns>
	protected virtual ValueTask<RunSummary> FailTestMethod(
		TContext ctxt,
		TTestMethod? testMethod,
		IReadOnlyCollection<TTestCase> testCases,
		object?[] constructorArguments,
		Exception exception) =>
			new(XunitRunnerHelper.FailTestCases(
				Guard.ArgumentNotNull(ctxt).MessageBus,
				ctxt.CancellationTokenSource,
				testCases,
				exception,
				sendTestMethodMessages: true
			));

	/// <summary>
	/// This method is called when an exception was thrown by <see cref="OnTestClassFinished"/>. By default, this
	/// sends <see cref="ErrorMessage"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/>. It must never throw an exception.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="exception">The exception that was thrown by <see cref="OnTestClassFinished"/>.</param>
	protected virtual ValueTask<bool> OnError(
		TContext ctxt,
		Exception exception) =>
			new(Guard.ArgumentNotNull(ctxt).MessageBus.QueueMessage(ErrorMessage.FromException(exception, ctxt.TestClass.TestCollection.TestAssembly.UniqueID)));

	/// <summary>
	/// This method is called when an exception was thrown while cleaning up, after the test class
	/// has run. By default, this sends <see cref="TestClassCleanupFailure"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown are
	/// converted into fatal exception messages (via <see cref="IErrorMessage"/>) and sent to the message
	/// bus in <paramref name="ctxt"/>.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="exception">The exception that caused the cleanup failure (may be an instance
	/// of <see cref="AggregateException"/> if more than one exception occurred).</param>
	/// <returns>Return <see langword="true"/> if test execution should continue; <see langword="false"/> if it should be shut down.</returns>
	protected virtual ValueTask<bool> OnTestClassCleanupFailure(
		TContext ctxt,
		Exception exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var (types, messages, stackTraces, indices, _) = ExceptionUtility.ExtractMetadata(exception);

		return new(ctxt.MessageBus.QueueMessage(new TestClassCleanupFailure
		{
			AssemblyUniqueID = ctxt.TestClass.TestCollection.TestAssembly.UniqueID,
			ExceptionParentIndices = indices,
			ExceptionTypes = types,
			Messages = messages,
			StackTraces = stackTraces,
			TestClassUniqueID = ctxt.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.TestClass.TestCollection.UniqueID,
		}));
	}

	/// <summary>
	/// This method will be called when the test class has finished running. By default, this sends
	/// <see cref="TestClassFinished"/>. Override this to enable any extensibility related to test
	/// class finish.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown will
	/// be reported as top-level exceptions. Any exceptions that are present in the aggregator (presumably
	/// from derived implementations of this method, <see cref="RunTestMethods"/>, or <see cref="RunTestMethod"/>)
	/// will invoke <see cref="OnTestClassCleanupFailure"/>.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="summary">The execution summary for the test class</param>
	/// <returns>Return <see langword="true"/> if test execution should continue; <see langword="false"/> if it should be shut down.</returns>
	protected virtual async ValueTask<bool> OnTestClassFinished(
		TContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		if (ctxt.Aggregator.HasExceptions)
		{
			var exception = ctxt.Aggregator.ToException()!;
			ctxt.Aggregator.Clear();

			if (!await ctxt.Aggregator.RunAsync(() => OnTestClassCleanupFailure(ctxt, exception), true))
				ctxt.CancellationTokenSource.Cancel();
		}

		return ctxt.MessageBus.QueueMessage(new TestClassFinished
		{
			AssemblyUniqueID = ctxt.TestClass.TestCollection.TestAssembly.UniqueID,
			ExecutionTime = summary.Time,
			FinishTime = DateTimeOffset.UtcNow,
			TestClassUniqueID = ctxt.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.TestClass.TestCollection.UniqueID,
			TestsFailed = summary.Failed,
			TestsNotRun = summary.NotRun,
			TestsSkipped = summary.Skipped,
			TestsTotal = summary.Total,
		});
	}

	/// <summary>
	/// This method will be called before the test class has started running. By default, this sends
	/// <see cref="TestClassStarting"/>. Override this to enable any extensibility related to test
	/// class start.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Initializing"/> and any exceptions thrown will
	/// contribute to test class failure (and will prevent the test class from running). Even if
	/// this method records exceptions, <see cref="OnTestClassFinished"/> will be called.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <returns>Return <see langword="true"/> if test execution should continue; <see langword="false"/> if it should be shut down.</returns>
	protected virtual ValueTask<bool> OnTestClassStarting(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return new(ctxt.MessageBus.QueueMessage(new TestClassStarting
		{
			AssemblyUniqueID = ctxt.TestClass.TestCollection.TestAssembly.UniqueID,
			StartTime = DateTimeOffset.UtcNow,
			TestClassName = Guard.ArgumentNotNull(ctxt).TestClass.TestClassName,
			TestClassNamespace = ctxt.TestClass.TestClassNamespace,
			TestClassSimpleName = ctxt.TestClass.TestClassSimpleName,
			TestClassUniqueID = ctxt.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.TestClass.TestCollection.UniqueID,
			Traits = ctxt.TestClass.Traits,
		}));
	}

	/// <summary>
	/// Please override <see cref="OrderTestMethods(TContext)"/> to order test methods, or override
	/// <see cref="TestMethodRunner{TContext, TTestMethod, TTestCase}.OrderTestCases(TContext)"/> to order
	/// test cases. This method is no longer used here and will be removed in the next major version.
	/// </summary>
	[Obsolete("Please override OrderTestMethods instead. This method will be removed in the next major version.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected virtual IReadOnlyCollection<TTestCase> OrderTestCases(TContext ctxt) =>
		Guard.ArgumentNotNull(ctxt).TestCases;

	/// <summary>
	/// Orders the test methods in the class. By default groups the test cases by method in order of
	/// appearance, and does not reorder the methods.
	/// </summary>
	/// <remarks>
	/// Override this to provide custom test class ordering.<br />
	/// <br />
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test class failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <returns>Test methods in run order (and associated, not-yet-ordered test cases).</returns>
	protected virtual List<(TTestMethod? Method, List<TTestCase> TestCases)> OrderTestMethods(TContext ctxt) =>
		OrderTestMethodsDefault(ctxt);

	static List<(TTestMethod? Method, List<TTestCase> TestCases)> OrderTestMethodsDefault(TContext ctxt) =>
		Guard.ArgumentNotNull(ctxt)
			.TestCases
			.GroupBy(tc => tc.TestMethod as TTestMethod, TestMethodComparer<TTestMethod>.Instance)
			.Select(grouping => (Class: grouping.Key, TestCases: grouping.ToList()))
			.ToList();

	/// <summary>
	/// Runs the tests in the test class.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected async ValueTask<RunSummary> Run(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		SetTestContext(ctxt, TestEngineStatus.Initializing, dispose: false);

		var summary = default(RunSummary);

		if (!await ctxt.Aggregator.RunAsync(() => OnTestClassStarting(ctxt), true))
			ctxt.CancellationTokenSource.Cancel();

		SetTestContext(ctxt, TestEngineStatus.Running, dispose: true);

		var startingException = ctxt.Aggregator.ToException();
		ctxt.Aggregator.Clear();

		if (!ctxt.CancellationTokenSource.IsCancellationRequested)
		{
			var logEnabled = TestEventSource.Log.IsEnabled();

			if (logEnabled)
				TestEventSource.Log.TestClassStart(ctxt.TestClass.TestClassName);

			try
			{
				summary = await ctxt.Aggregator.RunAsync(() => RunTestMethods(ctxt, startingException), default);
			}
			finally
			{
				if (logEnabled)
					TestEventSource.Log.TestClassStop(ctxt.TestClass.TestClassName);
			}
		}

		SetTestContext(ctxt, TestEngineStatus.CleaningUp, dispose: true);

		if (!await ctxt.Aggregator.RunAsync(() => OnTestClassFinished(ctxt, summary), true))
			ctxt.CancellationTokenSource.Cancel();

		if (ctxt.Aggregator.HasExceptions)
			if (!await ctxt.Aggregator.RunAsync(() => OnError(ctxt, ctxt.Aggregator.ToException()!), true))
				ctxt.CancellationTokenSource.Cancel();

		ctxt.Aggregator.Clear();

		TestContext.CurrentInternal.SafeDispose();

		return summary;
	}

	/// <summary>
	/// Runs the list of test methods. By default, orders the tests, groups them by method
	/// and runs them synchronously.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test class cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="exception">The exception that was caused during startup; should be used as an indicator that the
	/// downstream tests should fail with the provided exception rather than going through standard execution</param>
	/// <returns>Returns summary information about the tests that were run</returns>
	protected virtual async ValueTask<RunSummary> RunTestMethods(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var summary = new RunSummary();
		var orderedTestMethods = exception is null ? OrderTestMethods(ctxt) : OrderTestMethodsDefault(ctxt);
		object?[] constructorArguments;

		if (exception is null)
		{
			constructorArguments = await CreateTestClassConstructorArguments(ctxt);
			exception = ctxt.Aggregator.ToException();
			ctxt.Aggregator.Clear();
		}
		else
			constructorArguments = [];

		foreach (var testMethod in orderedTestMethods)
		{
			if (exception is not null)
				summary.Aggregate(await FailTestMethod(ctxt, testMethod.Method, testMethod.TestCases, constructorArguments, exception));
			else
				summary.Aggregate(await RunTestMethod(ctxt, testMethod.Method, testMethod.TestCases, constructorArguments));

			if (ctxt.CancellationTokenSource.IsCancellationRequested)
				break;
		}

		return summary;
	}

	/// <summary>
	/// Override this method to run the tests in an individual test method.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test class cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="testMethod">The test method that contains the test cases. May be <see langword="null"/> for test cases that do not
	/// support classes and methods.</param>
	/// <param name="testCases">The test cases to be run.</param>
	/// <param name="constructorArguments">The constructor arguments that will be used to create the test class.</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected abstract ValueTask<RunSummary> RunTestMethod(
		TContext ctxt,
		TTestMethod? testMethod,
		IReadOnlyCollection<TTestCase> testCases,
		object?[] constructorArguments
	);

	/// <summary>
	/// Sets the current <see cref="TestContext"/> for the current test class and the given test class status.
	/// </summary>
	/// <remarks>
	/// This method must never throw. Behavior is undefined if it does. Instead, exceptions that
	/// occur should be recorded in the aggregator in <paramref name="ctxt"/> and will be reflected
	/// in a way that's appropriate based on when this method is called.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="testClassStatus">The current test class status.</param>
	protected virtual void SetTestContext(
		TContext ctxt,
		TestEngineStatus testClassStatus)
	{
		Guard.ArgumentNotNull(ctxt);

		TestContext.SetForTestClass(ctxt.TestClass, testClassStatus, ctxt.CancellationTokenSource.Token);
	}

	void SetTestContext(
		TContext ctxt,
		TestEngineStatus testClassStatus,
		bool dispose)
	{
		var current = dispose ? TestContext.CurrentInternal : null;

		SetTestContext(ctxt, testClassStatus);

		current.SafeDispose();
	}
}
