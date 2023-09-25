#pragma warning disable CA1003 // The properties here are not intended to be .NET events
#pragma warning disable CA1713 // The properties here are not intended to be .NET events

using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common;

/// <summary>
/// Class that maps test framework execution messages to events.
/// </summary>
public class ExecutionEventSink : _IMessageSink
{

	/// <summary>
	/// Occurs when a <see cref="_AfterTestFinished"/> message is received.
	/// </summary>
	public event MessageHandler<_AfterTestFinished>? AfterTestFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="_AfterTestStarting"/> message is received.
	/// </summary>
	public event MessageHandler<_AfterTestStarting>? AfterTestStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="_BeforeTestFinished"/> message is received.
	/// </summary>
	public event MessageHandler<_BeforeTestFinished>? BeforeTestFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="_BeforeTestStarting"/> message is received.
	/// </summary>
	public event MessageHandler<_BeforeTestStarting>? BeforeTestStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestAssemblyCleanupFailure"/> message is received.
	/// </summary>
	public event MessageHandler<_TestAssemblyCleanupFailure>? TestAssemblyCleanupFailureEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestAssemblyFinished"/> message is received.
	/// </summary>
	public event MessageHandler<_TestAssemblyFinished>? TestAssemblyFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestAssemblyStarting"/> message is received.
	/// </summary>
	public event MessageHandler<_TestAssemblyStarting>? TestAssemblyStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestCaseCleanupFailure"/> message is received.
	/// </summary>
	public event MessageHandler<_TestCaseCleanupFailure>? TestCaseCleanupFailureEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestCaseFinished"/> message is received.
	/// </summary>
	public event MessageHandler<_TestCaseFinished>? TestCaseFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestCaseStarting"/> message is received.
	/// </summary>
	public event MessageHandler<_TestCaseStarting>? TestCaseStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestClassCleanupFailure"/> message is received.
	/// </summary>
	public event MessageHandler<_TestClassCleanupFailure>? TestClassCleanupFailureEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestClassConstructionFinished"/> message is received.
	/// </summary>
	public event MessageHandler<_TestClassConstructionFinished>? TestClassConstructionFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestClassConstructionStarting"/> message is received.
	/// </summary>
	public event MessageHandler<_TestClassConstructionStarting>? TestClassConstructionStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestClassDisposeFinished"/> message is received.
	/// </summary>
	public event MessageHandler<_TestClassDisposeFinished>? TestClassDisposeFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestClassDisposeStarting"/> message is received.
	/// </summary>
	public event MessageHandler<_TestClassDisposeStarting>? TestClassDisposeStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestClassFinished"/> message is received.
	/// </summary>
	public event MessageHandler<_TestClassFinished>? TestClassFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestClassStarting"/> message is received.
	/// </summary>
	public event MessageHandler<_TestClassStarting>? TestClassStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestCleanupFailure"/> message is received.
	/// </summary>
	public event MessageHandler<_TestCleanupFailure>? TestCleanupFailureEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestCollectionCleanupFailure"/> message is received.
	/// </summary>
	public event MessageHandler<_TestCollectionCleanupFailure>? TestCollectionCleanupFailureEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestCollectionFinished"/> message is received.
	/// </summary>
	public event MessageHandler<_TestCollectionFinished>? TestCollectionFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestCollectionStarting"/> message is received.
	/// </summary>
	public event MessageHandler<_TestCollectionStarting>? TestCollectionStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestFailed"/> message is received.
	/// </summary>
	public event MessageHandler<_TestFailed>? TestFailedEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestFinished"/> message is received.
	/// </summary>
	public event MessageHandler<_TestFinished>? TestFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestMethodCleanupFailure"/> message is received.
	/// </summary>
	public event MessageHandler<_TestMethodCleanupFailure>? TestMethodCleanupFailureEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestMethodFinished"/> message is received.
	/// </summary>
	public event MessageHandler<_TestMethodFinished>? TestMethodFinishedEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestMethodStarting"/> message is received.
	/// </summary>
	public event MessageHandler<_TestMethodStarting>? TestMethodStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestNotRun"/> message is received.
	/// </summary>
	public event MessageHandler<_TestNotRun>? TestNotRunEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestOutput"/> message is received.
	/// </summary>
	public event MessageHandler<_TestOutput>? TestOutputEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestPassed"/> message is received.
	/// </summary>
	public event MessageHandler<_TestPassed>? TestPassedEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestSkipped"/> message is received.
	/// </summary>
	public event MessageHandler<_TestSkipped>? TestSkippedEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestStarting"/> message is received.
	/// </summary>
	public event MessageHandler<_TestStarting>? TestStartingEvent;

	/// <inheritdoc/>
	public bool OnMessage(_MessageSinkMessage message)
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
