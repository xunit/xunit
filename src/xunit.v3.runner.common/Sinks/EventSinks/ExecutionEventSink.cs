#pragma warning disable CA1003 // The properties here are not intended to be .NET events
#pragma warning disable CA1713 // The properties here are not intended to be .NET events

using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Class that maps test framework execution messages to events.
/// </summary>
public class ExecutionEventSink : IMessageSink
{
	/// <summary>
	/// Occurs when a <see cref="IAfterTestFinished"/> message is received.
	/// </summary>
	public event MessageHandler<IAfterTestFinished>? AfterTestFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="IAfterTestStarting"/> message is received.
	/// </summary>
	public event MessageHandler<IAfterTestStarting>? AfterTestStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="IBeforeTestFinished"/> message is received.
	/// </summary>
	public event MessageHandler<IBeforeTestFinished>? BeforeTestFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="IBeforeTestStarting"/> message is received.
	/// </summary>
	public event MessageHandler<IBeforeTestStarting>? BeforeTestStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestAssemblyCleanupFailure"/> message is received.
	/// </summary>
	public event MessageHandler<ITestAssemblyCleanupFailure>? TestAssemblyCleanupFailureEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestAssemblyFinished"/> message is received.
	/// </summary>
	public event MessageHandler<ITestAssemblyFinished>? TestAssemblyFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestAssemblyStarting"/> message is received.
	/// </summary>
	public event MessageHandler<ITestAssemblyStarting>? TestAssemblyStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestCaseCleanupFailure"/> message is received.
	/// </summary>
	public event MessageHandler<ITestCaseCleanupFailure>? TestCaseCleanupFailureEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestCaseFinished"/> message is received.
	/// </summary>
	public event MessageHandler<ITestCaseFinished>? TestCaseFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestCaseStarting"/> message is received.
	/// </summary>
	public event MessageHandler<ITestCaseStarting>? TestCaseStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestClassCleanupFailure"/> message is received.
	/// </summary>
	public event MessageHandler<ITestClassCleanupFailure>? TestClassCleanupFailureEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestClassConstructionFinished"/> message is received.
	/// </summary>
	public event MessageHandler<ITestClassConstructionFinished>? TestClassConstructionFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestClassConstructionStarting"/> message is received.
	/// </summary>
	public event MessageHandler<ITestClassConstructionStarting>? TestClassConstructionStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestClassDisposeFinished"/> message is received.
	/// </summary>
	public event MessageHandler<ITestClassDisposeFinished>? TestClassDisposeFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestClassDisposeStarting"/> message is received.
	/// </summary>
	public event MessageHandler<ITestClassDisposeStarting>? TestClassDisposeStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestClassFinished"/> message is received.
	/// </summary>
	public event MessageHandler<ITestClassFinished>? TestClassFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestClassStarting"/> message is received.
	/// </summary>
	public event MessageHandler<ITestClassStarting>? TestClassStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestCleanupFailure"/> message is received.
	/// </summary>
	public event MessageHandler<ITestCleanupFailure>? TestCleanupFailureEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestCollectionCleanupFailure"/> message is received.
	/// </summary>
	public event MessageHandler<ITestCollectionCleanupFailure>? TestCollectionCleanupFailureEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestCollectionFinished"/> message is received.
	/// </summary>
	public event MessageHandler<ITestCollectionFinished>? TestCollectionFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestCollectionStarting"/> message is received.
	/// </summary>
	public event MessageHandler<ITestCollectionStarting>? TestCollectionStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestFailed"/> message is received.
	/// </summary>
	public event MessageHandler<ITestFailed>? TestFailedEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestFinished"/> message is received.
	/// </summary>
	public event MessageHandler<ITestFinished>? TestFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestMethodCleanupFailure"/> message is received.
	/// </summary>
	public event MessageHandler<ITestMethodCleanupFailure>? TestMethodCleanupFailureEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestMethodFinished"/> message is received.
	/// </summary>
	public event MessageHandler<ITestMethodFinished>? TestMethodFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestMethodStarting"/> message is received.
	/// </summary>
	public event MessageHandler<ITestMethodStarting>? TestMethodStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestNotRun"/> message is received.
	/// </summary>
	public event MessageHandler<ITestNotRun>? TestNotRunEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestOutput"/> message is received.
	/// </summary>
	public event MessageHandler<ITestOutput>? TestOutputEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestPassed"/> message is received.
	/// </summary>
	public event MessageHandler<ITestPassed>? TestPassedEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestSkipped"/> message is received.
	/// </summary>
	public event MessageHandler<ITestSkipped>? TestSkippedEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestStarting"/> message is received.
	/// </summary>
	public event MessageHandler<ITestStarting>? TestStartingEvent;

	/// <inheritdoc/>
	public bool OnMessage(IMessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		return
			message.DispatchWhen(AfterTestFinishedEvent) &&
			message.DispatchWhen(AfterTestStartingEvent) &&
			message.DispatchWhen(BeforeTestFinishedEvent) &&
			message.DispatchWhen(BeforeTestStartingEvent) &&
			message.DispatchWhen(TestAssemblyCleanupFailureEvent) &&
			message.DispatchWhen(TestAssemblyFinishedEvent) &&
			message.DispatchWhen(TestAssemblyStartingEvent) &&
			message.DispatchWhen(TestCaseCleanupFailureEvent) &&
			message.DispatchWhen(TestCaseFinishedEvent) &&
			message.DispatchWhen(TestCaseStartingEvent) &&
			message.DispatchWhen(TestClassCleanupFailureEvent) &&
			message.DispatchWhen(TestClassConstructionFinishedEvent) &&
			message.DispatchWhen(TestClassConstructionStartingEvent) &&
			message.DispatchWhen(TestClassDisposeFinishedEvent) &&
			message.DispatchWhen(TestClassDisposeStartingEvent) &&
			message.DispatchWhen(TestClassFinishedEvent) &&
			message.DispatchWhen(TestClassStartingEvent) &&
			message.DispatchWhen(TestCleanupFailureEvent) &&
			message.DispatchWhen(TestCollectionCleanupFailureEvent) &&
			message.DispatchWhen(TestCollectionFinishedEvent) &&
			message.DispatchWhen(TestCollectionStartingEvent) &&
			message.DispatchWhen(TestFailedEvent) &&
			message.DispatchWhen(TestFinishedEvent) &&
			message.DispatchWhen(TestMethodCleanupFailureEvent) &&
			message.DispatchWhen(TestMethodFinishedEvent) &&
			message.DispatchWhen(TestMethodStartingEvent) &&
			message.DispatchWhen(TestNotRunEvent) &&
			message.DispatchWhen(TestOutputEvent) &&
			message.DispatchWhen(TestPassedEvent) &&
			message.DispatchWhen(TestSkippedEvent) &&
			message.DispatchWhen(TestStartingEvent);
	}
}
