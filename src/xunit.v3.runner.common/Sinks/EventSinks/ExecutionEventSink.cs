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
	/// Occurs when a <see cref="AfterTestFinished"/> message is received.
	/// </summary>
	public event MessageHandler<AfterTestFinished>? AfterTestFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="AfterTestStarting"/> message is received.
	/// </summary>
	public event MessageHandler<AfterTestStarting>? AfterTestStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="BeforeTestFinished"/> message is received.
	/// </summary>
	public event MessageHandler<BeforeTestFinished>? BeforeTestFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="BeforeTestStarting"/> message is received.
	/// </summary>
	public event MessageHandler<BeforeTestStarting>? BeforeTestStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="TestAssemblyCleanupFailure"/> message is received.
	/// </summary>
	public event MessageHandler<TestAssemblyCleanupFailure>? TestAssemblyCleanupFailureEvent;

	/// <summary>
	/// Occurs when a <see cref="TestAssemblyFinished"/> message is received.
	/// </summary>
	public event MessageHandler<TestAssemblyFinished>? TestAssemblyFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="TestAssemblyStarting"/> message is received.
	/// </summary>
	public event MessageHandler<TestAssemblyStarting>? TestAssemblyStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="TestCaseCleanupFailure"/> message is received.
	/// </summary>
	public event MessageHandler<TestCaseCleanupFailure>? TestCaseCleanupFailureEvent;

	/// <summary>
	/// Occurs when a <see cref="TestCaseFinished"/> message is received.
	/// </summary>
	public event MessageHandler<TestCaseFinished>? TestCaseFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="TestCaseStarting"/> message is received.
	/// </summary>
	public event MessageHandler<TestCaseStarting>? TestCaseStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="TestClassCleanupFailure"/> message is received.
	/// </summary>
	public event MessageHandler<TestClassCleanupFailure>? TestClassCleanupFailureEvent;

	/// <summary>
	/// Occurs when a <see cref="TestClassConstructionFinished"/> message is received.
	/// </summary>
	public event MessageHandler<TestClassConstructionFinished>? TestClassConstructionFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="TestClassConstructionStarting"/> message is received.
	/// </summary>
	public event MessageHandler<TestClassConstructionStarting>? TestClassConstructionStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="TestClassDisposeFinished"/> message is received.
	/// </summary>
	public event MessageHandler<TestClassDisposeFinished>? TestClassDisposeFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="TestClassDisposeStarting"/> message is received.
	/// </summary>
	public event MessageHandler<TestClassDisposeStarting>? TestClassDisposeStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="TestClassFinished"/> message is received.
	/// </summary>
	public event MessageHandler<TestClassFinished>? TestClassFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="TestClassStarting"/> message is received.
	/// </summary>
	public event MessageHandler<TestClassStarting>? TestClassStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="TestCleanupFailure"/> message is received.
	/// </summary>
	public event MessageHandler<TestCleanupFailure>? TestCleanupFailureEvent;

	/// <summary>
	/// Occurs when a <see cref="TestCollectionCleanupFailure"/> message is received.
	/// </summary>
	public event MessageHandler<TestCollectionCleanupFailure>? TestCollectionCleanupFailureEvent;

	/// <summary>
	/// Occurs when a <see cref="TestCollectionFinished"/> message is received.
	/// </summary>
	public event MessageHandler<TestCollectionFinished>? TestCollectionFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="TestCollectionStarting"/> message is received.
	/// </summary>
	public event MessageHandler<TestCollectionStarting>? TestCollectionStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="TestFailed"/> message is received.
	/// </summary>
	public event MessageHandler<TestFailed>? TestFailedEvent;

	/// <summary>
	/// Occurs when a <see cref="TestFinished"/> message is received.
	/// </summary>
	public event MessageHandler<TestFinished>? TestFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="TestMethodCleanupFailure"/> message is received.
	/// </summary>
	public event MessageHandler<TestMethodCleanupFailure>? TestMethodCleanupFailureEvent;

	/// <summary>
	/// Occurs when a <see cref="TestMethodFinished"/> message is received.
	/// </summary>
	public event MessageHandler<TestMethodFinished>? TestMethodFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="TestMethodStarting"/> message is received.
	/// </summary>
	public event MessageHandler<TestMethodStarting>? TestMethodStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="TestNotRun"/> message is received.
	/// </summary>
	public event MessageHandler<TestNotRun>? TestNotRunEvent;

	/// <summary>
	/// Occurs when a <see cref="TestOutput"/> message is received.
	/// </summary>
	public event MessageHandler<TestOutput>? TestOutputEvent;

	/// <summary>
	/// Occurs when a <see cref="TestPassed"/> message is received.
	/// </summary>
	public event MessageHandler<TestPassed>? TestPassedEvent;

	/// <summary>
	/// Occurs when a <see cref="TestSkipped"/> message is received.
	/// </summary>
	public event MessageHandler<TestSkipped>? TestSkippedEvent;

	/// <summary>
	/// Occurs when a <see cref="TestStarting"/> message is received.
	/// </summary>
	public event MessageHandler<TestStarting>? TestStartingEvent;

	/// <inheritdoc/>
	public bool OnMessage(MessageSinkMessage message)
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
