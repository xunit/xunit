using System;
using System.Globalization;
using System.Threading;
using Xunit.Runner.Common;
using Xunit.Runner.v3;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base class for crash detection, to be used by <see cref="Xunit3"/> during discovery and execution
/// so that we can always ensure runners get starting and finished messages.
/// </summary>
/// <typeparam name="TStart"></typeparam>
/// <typeparam name="TFinish"></typeparam>
/// <param name="projectAssembly"></param>
/// <param name="innerSink"></param>
public abstract class CrashDetectionSinkBase<TStart, TFinish>(
	XunitProjectAssembly projectAssembly,
	IMessageSink innerSink) :
		LongLivedMarshalByRefObject, IMessageSink
			where TStart : IMessageSinkMessage, ITestAssemblyMessage
			where TFinish : IMessageSinkMessage
{
	DateTimeOffset lastMessageReceived = DateTimeOffset.MinValue;
	bool stopProcessing;

	/// <summary>
	/// Gets the finish message, if one was seen.
	/// </summary>
	protected TFinish? Finish { get; private set; }

	/// <summary>
	/// This override is for testing purposes.
	/// </summary>
	protected virtual int FinishWaitMilliseconds =>
		5_000;

	/// <summary>
	/// Gets the inner sink for message delegation.
	/// </summary>
	protected IMessageSink InnerSink =>
		innerSink;

	/// <summary>
	/// Gets the project assembly.
	/// </summary>
	protected XunitProjectAssembly ProjectAssembly =>
		projectAssembly;

	/// <summary>
	/// Gets the start message, if one was seen.
	/// </summary>
	protected TStart? Start { get; private set; }

	/// <inheritdoc/>
	public bool OnMessage(IMessageSinkMessage message)
	{
		if (stopProcessing)
			return true;

		lastMessageReceived = DateTimeOffset.UtcNow;

		if (message is TStart start)
			Start = start;
		else if (message is TFinish finish)
			Finish = finish;

		return InnerSink.OnMessage(message);
	}

	/// <summary>
	/// Call this message to ensure the starting and finished messages were sent,
	/// and also send an <see cref="IErrorMessage"/> if it appears that the test
	/// process crashed rather than cleaning up appropriately.
	/// </summary>
	/// <param name="exitCode">The exit code from the test process, if known</param>
	public void OnProcessFinished(int? exitCode)
	{
		if (lastMessageReceived == DateTimeOffset.MinValue)
			lastMessageReceived = DateTimeOffset.UtcNow;

		// Give the finish message a little time to arrive
		while (true)
		{
			// If we saw the finish message, there's nothing to do
			if (Finish is not null)
				return;

			if ((DateTimeOffset.UtcNow - lastMessageReceived).TotalMilliseconds >= FinishWaitMilliseconds)
				break;

			Thread.Yield();
		}

		stopProcessing = true;

		string? assemblyUniqueID;

		// Did we see the start message? If not, then the process probably didn't even start properly
		if (Start is not null)
			assemblyUniqueID = Start.AssemblyUniqueID;
		else
		{
			assemblyUniqueID = UniqueIDGenerator.ForAssembly(projectAssembly.AssemblyFileName, projectAssembly.ConfigFileName);
			SendStart(assemblyUniqueID);
		}

		// Send the error message
		var message =
			exitCode is not null
				? string.Format(CultureInfo.CurrentCulture, "Test process crashed with exit code {0}.", exitCode)
				: "Test process crashed or communication channel was lost.";

		InnerSink.OnMessage(new ErrorMessage
		{
			ExceptionParentIndices = [-1],
			ExceptionTypes = ["Xunit.Sdk.TestPipelineException"],
			Messages = [message],
			StackTraces = [null],
		});

		// Send the discovery complete message
		SendFinish(assemblyUniqueID);
	}

	/// <summary>
	/// Implement this to send the finished message.
	/// </summary>
	protected abstract void SendFinish(string assemblyUniqueID);

	/// <summary>
	/// Implement this to send the starting message.
	/// </summary>
	protected abstract void SendStart(string assemblyUniqueID);
}
