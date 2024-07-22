using System;
using System.Collections.Generic;
using System.Linq;
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
	/// set of arguments. Override to find the arguments for the constructor.
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
	/// This method is called when an exception was thrown while cleaning up, after the test class
	/// has run. This will typically send a "test class cleanup failure" message (like
	/// <see cref="ITestClassCleanupFailure"/>).
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown are
	/// converted into fatal exception messages (via <see cref="IErrorMessage"/>) and sent to the message
	/// bus in <paramref name="ctxt"/>.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="exception">The exception that caused the cleanup failure (may be an instance
	/// of <see cref="AggregateException"/> if more than one exception occurred).</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<bool> OnTestClassCleanupFailure(
		TContext ctxt,
		Exception exception);

	/// <summary>
	/// This method will be called when the test class has finished running. This will typically send
	/// a "test class finished" message (like <see cref="ITestClassFinished"/>) as well as enabling any
	/// extensibility related to test class finish.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown will
	/// contribute to test class cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="summary">The execution summary for the test class</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<bool> OnTestClassFinished(
		TContext ctxt,
		RunSummary summary);

	/// <summary>
	/// This method will be called before the test class has started running. This will typically send
	/// a "test class starting" message (like <see cref="ITestClassStarting"/>) as well as enabling any
	/// extensibility related to test class start.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Initializing"/> and any exceptions thrown will
	/// contribute to test class failure (and will prevent the test class from running). Even if
	/// this method records exceptions, <see cref="OnTestClassFinished"/> will be called.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<bool> OnTestClassStarting(TContext ctxt);

	/// <summary>
	/// Orders the test cases in the class. By default does not re-order the test cases.
	/// Override this to provide custom test case ordering.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test class failure
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	protected virtual IReadOnlyCollection<TTestCase> OrderTestCases(TContext ctxt) =>
		Guard.ArgumentNotNull(ctxt).TestCases;

	/// <summary>
	/// Runs the tests in the test class.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected async ValueTask<RunSummary> RunAsync(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		SetTestContext(ctxt, TestEngineStatus.Initializing);

		var summary = default(RunSummary);

		if (!await ctxt.Aggregator.RunAsync(() => OnTestClassStarting(ctxt), true))
			ctxt.CancellationTokenSource.Cancel();

		SetTestContext(ctxt, TestEngineStatus.Running);

		var startingException = ctxt.Aggregator.ToException();
		ctxt.Aggregator.Clear();

		if (!ctxt.CancellationTokenSource.IsCancellationRequested)
			summary = await ctxt.Aggregator.RunAsync(() => RunTestMethodsAsync(ctxt, startingException), default);

		SetTestContext(ctxt, TestEngineStatus.CleaningUp);

		if (!await ctxt.Aggregator.RunAsync(() => OnTestClassFinished(ctxt, summary), true))
			ctxt.CancellationTokenSource.Cancel();

		if (ctxt.Aggregator.HasExceptions)
		{
			var exception = ctxt.Aggregator.ToException()!;
			ctxt.Aggregator.Clear();

			if (!await ctxt.Aggregator.RunAsync(() => OnTestClassCleanupFailure(ctxt, exception), true))
				ctxt.CancellationTokenSource.Cancel();

			if (ctxt.Aggregator.HasExceptions)
				ctxt.MessageBus.QueueMessage(ErrorMessage.FromException(ctxt.Aggregator.ToException()!));

			ctxt.Aggregator.Clear();
		}

		return summary;
	}

	/// <summary>
	/// Runs the list of test methods. By default, orders the tests, groups them by method
	/// and runs them synchronously.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="exception">The exception that was caused during startup; should be used as an indicator that the
	/// downstream tests should fail with the provided exception rather than going through standard execution</param>
	/// <returns>Returns summary information about the tests that were run</returns>
	protected virtual async ValueTask<RunSummary> RunTestMethodsAsync(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var summary = new RunSummary();
		IReadOnlyCollection<TTestCase> orderedTestCases;
		object?[] constructorArguments;

		if (exception is null)
		{
			orderedTestCases = OrderTestCases(ctxt);
			constructorArguments = await CreateTestClassConstructorArguments(ctxt);
			exception = ctxt.Aggregator.ToException();
			ctxt.Aggregator.Clear();
		}
		else
		{
			orderedTestCases = ctxt.TestCases;
			constructorArguments = [];
		}

		foreach (var method in orderedTestCases.GroupBy(tc => tc.TestMethod, TestMethodComparer.Instance))
		{
			summary.Aggregate(await RunTestMethodAsync(ctxt, method.Key as TTestMethod, method.CastOrToReadOnlyCollection(), constructorArguments, exception));
			if (ctxt.CancellationTokenSource.IsCancellationRequested)
				break;
		}

		return summary;
	}

	/// <summary>
	/// Override this method to run the tests in an individual test method.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="testMethod">The test method that contains the test cases. May be <c>null</c> for test cases that do not
	/// support classes and methods.</param>
	/// <param name="testCases">The test cases to be run.</param>
	/// <param name="constructorArguments">The constructor arguments that will be used to create the test class.</param>
	/// <param name="exception">The exception that was caused during startup; should be used as an indicator that the
	/// downstream tests should fail with the provided exception rather than going through standard execution</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected abstract ValueTask<RunSummary> RunTestMethodAsync(
		TContext ctxt,
		TTestMethod? testMethod,
		IReadOnlyCollection<TTestCase> testCases,
		object?[] constructorArguments,
		Exception? exception
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
}
