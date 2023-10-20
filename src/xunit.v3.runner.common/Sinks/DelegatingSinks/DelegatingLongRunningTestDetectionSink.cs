using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common;

/// <summary>
/// A delegating implementation of <see cref="IExecutionSink"/> which detects and reports when
/// tests have become long-running (during otherwise idle time).
/// </summary>
public class DelegatingLongRunningTestDetectionSink : IExecutionSink
{
	readonly Action<LongRunningTestsSummary> callback;
	readonly Dictionary<string, (_ITestCaseMetadata metadata, DateTime startTime)> executingTestCases = new();
	readonly ExecutionEventSink executionSink = new();
	readonly IExecutionSink innerSink;
	DateTime lastTestActivity;
	readonly TimeSpan longRunningTestTime;
	ManualResetEvent? stopEvent;

	/// <summary>
	/// Initializes a new instance of the <see cref="DelegatingLongRunningTestDetectionSink"/> class, with
	/// long running test messages being delivered as <see cref="_DiagnosticMessage"/> instances to the
	/// provided diagnostic message sink.
	/// </summary>
	/// <param name="innerSink">The inner sink to delegate to.</param>
	/// <param name="longRunningTestTime">The minimum amount of time a test runs to be considered long running.</param>
	/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
	public DelegatingLongRunningTestDetectionSink(
		IExecutionSink innerSink,
		TimeSpan longRunningTestTime,
		_IMessageSink diagnosticMessageSink)
			: this(innerSink, longRunningTestTime, summary => DispatchLongRunningTestsSummaryAsDiagnosticMessage(summary, diagnosticMessageSink))
	{
		Guard.ArgumentNotNull(diagnosticMessageSink);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DelegatingLongRunningTestDetectionSink"/> class, with
	/// long running test messages being delivered as <see cref="LongRunningTestsSummary"/> to the
	/// provided callback.
	/// </summary>
	/// <param name="innerSink">The inner sink to delegate to.</param>
	/// <param name="longRunningTestTime">The minimum amount of time a test runs to be considered long running.</param>
	/// <param name="callback">The callback to dispatch messages to.</param>
	public DelegatingLongRunningTestDetectionSink(
		IExecutionSink innerSink,
		TimeSpan longRunningTestTime,
		Action<LongRunningTestsSummary> callback)
	{
		Guard.ArgumentNotNull(innerSink);
		Guard.ArgumentValid("Long running test time must be at least 1 second", longRunningTestTime >= TimeSpan.FromSeconds(1), nameof(longRunningTestTime));
		Guard.ArgumentNotNull(callback);

		this.innerSink = innerSink;
		this.longRunningTestTime = longRunningTestTime;
		this.callback = callback;

		executionSink.TestAssemblyFinishedEvent += HandleTestAssemblyFinished;
		executionSink.TestAssemblyStartingEvent += HandleTestAssemblyStarting;
		executionSink.TestCaseFinishedEvent += HandleTestCaseFinished;
		executionSink.TestCaseStartingEvent += HandleTestCaseStarting;
	}

	/// <inheritdoc/>
	public ExecutionSummary ExecutionSummary => innerSink.ExecutionSummary;

	/// <inheritdoc/>
	public ManualResetEvent Finished => innerSink.Finished;

	/// <summary>
	/// Returns the current time in UTC. Overrideable for testing purposes.
	/// </summary>
	protected virtual DateTime UtcNow => DateTime.UtcNow;

	static void DispatchLongRunningTestsSummaryAsDiagnosticMessage(
		LongRunningTestsSummary summary,
		_IMessageSink diagnosticMessageSink)
	{
		var messages = summary.TestCases.Select(pair => string.Format(CultureInfo.CurrentCulture, "Long running test: '{0}' (elapsed: {1:hh\\:mm\\:ss})", pair.Key.TestCaseDisplayName, pair.Value));
		var message = string.Join(Environment.NewLine, messages.ToArray());

		diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = message });
		diagnosticMessageSink.OnMessage(new _InternalDiagnosticMessage { Message = message });
	}

	/// <inheritdoc/>
	public virtual void Dispose()
	{
		GC.SuppressFinalize(this);

		((IDisposable?)stopEvent)?.Dispose();
		innerSink.Dispose();
	}

	void HandleTestAssemblyFinished(MessageHandlerArgs<_TestAssemblyFinished> args)
	{
		stopEvent?.Set();
	}

	void HandleTestAssemblyStarting(MessageHandlerArgs<_TestAssemblyStarting> args)
	{
		stopEvent = new ManualResetEvent(initialState: false);
		lastTestActivity = UtcNow;
		ThreadPool.QueueUserWorkItem(ThreadWorker);
	}

	void HandleTestCaseFinished(MessageHandlerArgs<_TestCaseFinished> args)
	{
		lock (executingTestCases)
		{
			executingTestCases.Remove(args.Message.TestCaseUniqueID);
			lastTestActivity = UtcNow;
		}
	}

	void HandleTestCaseStarting(MessageHandlerArgs<_TestCaseStarting> args)
	{
		lock (executingTestCases)
			executingTestCases.Add(args.Message.TestCaseUniqueID, (args.Message, UtcNow));
	}

	/// <inheritdoc/>
	public bool OnMessage(_MessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		var result = executionSink.OnMessage(message);
		result = innerSink.OnMessage(message) && result;
		return result;
	}

	void SendLongRunningMessage()
	{
		Dictionary<_ITestCaseMetadata, TimeSpan> longRunningTestCases;
		lock (executingTestCases)
		{
			var now = UtcNow;

			longRunningTestCases =
				executingTestCases
					.Where(kvp => (now - kvp.Value.startTime) >= longRunningTestTime)
					.ToDictionary(k => k.Value.metadata, v => now - v.Value.startTime);
		}

		if (longRunningTestCases.Count > 0)
			callback(new LongRunningTestsSummary(longRunningTestTime, longRunningTestCases));
	}

	void ThreadWorker(object? _)
	{
		// Fire the loop approximately every 1/10th of our delay time, but no more frequently than every
		// second (so we don't over-fire the timer). This should give us reasonable precision for the
		// requested delay time, without going crazy to check for long-running tests.

		var delayTime = (int)Math.Max(1000, longRunningTestTime.TotalMilliseconds / 10);

		while (true)
		{
			if (WaitForStopEvent(delayTime))
				return;

			var now = UtcNow;
			if (now - lastTestActivity >= longRunningTestTime)
			{
				SendLongRunningMessage();
				lastTestActivity = now;
			}
		}
	}

	/// <summary>
	/// Performs a Task-safe delay. Overrideable for testing purposes.
	/// </summary>
	protected virtual bool WaitForStopEvent(int millionsecondsDelay) =>
		stopEvent?.WaitOne(millionsecondsDelay) ?? false;
}
