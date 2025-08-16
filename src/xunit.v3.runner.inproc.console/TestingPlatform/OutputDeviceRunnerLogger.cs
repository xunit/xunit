using System;
using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.OutputDevice;
using Xunit.Runner.Common;

namespace Xunit.Runner.InProc.SystemConsole.TestingPlatform;

/// <summary>
/// Implementation of <see cref="IRunnerLogger"/> and <see cref="IOutputDeviceDataProducer"/>
/// that sends log messages to the <see cref="IOutputDevice"/>. It optionally wraps an existing
/// implementation of <see cref="IRunnerLogger"/> to forward all messages to. The <paramref name="rawOnly"/>
/// flag is used to only output raw log messages (typically used by reporter message handlers that need
/// to output text that is designed to be consumed programatically).
/// </summary>
/// <remarks>
/// This class is an implementation detail for Microsoft.Testing.Platform that is public for testing purposes.
/// Use this class at your own risk, as breaking changes may occur as needed.
/// </remarks>
public sealed class OutputDeviceRunnerLogger(
	IOutputDevice outputDevice,
	IRunnerLogger? innerLogger,
	bool rawOnly) :
		OutputDeviceDataProducerBase("output device runner logger", "b7b01fae-f36c-492e-b561-b3e4cad62203"), IRunnerLogger
{
	/// <inheritdoc/>
	public object LockObject { get; } = new();

	/// <inheritdoc/>
	public void LogError(
		StackFrameInfo stackFrame,
		string message)
	{
		if (!rawOnly)
			outputDevice.DisplayAsync(this, ToMessageWithColor(message, ConsoleColor.Red), default).SpinWait();

		innerLogger?.LogError(stackFrame, message);
	}

	/// <inheritdoc/>
	public void LogImportantMessage(
		StackFrameInfo stackFrame,
		string message)
	{
		if (!rawOnly)
			outputDevice.DisplayAsync(this, new TextOutputDeviceData(message), default).SpinWait();

		innerLogger?.LogImportantMessage(stackFrame, message);
	}

	/// <inheritdoc/>
	public void LogMessage(
		StackFrameInfo stackFrame,
		string message)
	{
		if (!rawOnly)
			outputDevice.DisplayAsync(this, ToMessageWithColor(message, ConsoleColor.DarkGray), default).SpinWait();

		innerLogger?.LogMessage(stackFrame, message);
	}

	/// <inheritdoc/>
	public void LogRaw(string message)
	{
		outputDevice.DisplayAsync(this, new TextOutputDeviceData(message), default).SpinWait();
		innerLogger?.LogRaw(message);
	}

	/// <inheritdoc/>
	public void LogWarning(
		StackFrameInfo stackFrame,
		string message)
	{
		if (!rawOnly)
			outputDevice.DisplayAsync(this, ToMessageWithColor(message, ConsoleColor.Yellow), default).SpinWait();

		innerLogger?.LogWarning(stackFrame, message);
	}

	/// <inheritdoc/>
	public void WaitForAcknowledgment() =>
		innerLogger?.WaitForAcknowledgment();
}
