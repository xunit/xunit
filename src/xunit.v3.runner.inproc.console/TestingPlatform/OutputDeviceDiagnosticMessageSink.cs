using System;
using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.Logging;
using Microsoft.Testing.Platform.OutputDevice;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.InProc.SystemConsole.TestingPlatform;

/// <summary>
/// Implementation of <see cref="IMessageSink"/> and <see cref="IOutputDeviceDataProducer"/> that
/// forwards diagnostic messages to the <see cref="IOutputDevice"/>. It wraps an existing implementation
/// of <see cref="IMessageSink"/> to forward all messages to.
/// </summary>
/// <remarks>
/// This class is an implementation detail for Microsoft.Testing.Platform that is public for testing purposes.
/// Use this class at your own risk, as breaking changes may occur as needed.
/// </remarks>
public sealed class OutputDeviceDiagnosticMessageSink(
	IOutputDevice outputDevice,
	bool diagnosticMessages,
	bool internalDiagnosticMessages,
	IMessageSink innerSink) :
		OutputDeviceDataProducerBase("output device diagnostic message sink", "e85050db-8ef2-4ef1-895b-8c4b620025e2"), IMessageSink
{
	/// <summary>
	/// Gets the inner message sink.
	/// </summary>
	public IMessageSink InnerSink { get; } = Guard.ArgumentNotNull(innerSink);

	/// <inheritdoc/>
	public bool OnMessage(IMessageSinkMessage message)
	{
		if (diagnosticMessages && message is IDiagnosticMessage diagnosticMessage)
			outputDevice.DisplayAsync(this, ToMessageWithColor(diagnosticMessage.Message, ConsoleColor.Yellow), default).SpinWait();
		if (internalDiagnosticMessages && message is IInternalDiagnosticMessage internalDiagnosticMessage)
			outputDevice.DisplayAsync(this, ToMessageWithColor(internalDiagnosticMessage.Message, ConsoleColor.DarkGray), default).SpinWait();

		return InnerSink.OnMessage(message);
	}

	/// <summary>
	/// Factory function for the message sink
	/// </summary>
	public static IMessageSink? TryCreate(
		ILogger logger,
		IOutputDevice outputDevice,
		bool diagnosticMessages,
		bool internalDiagnosticMessages)
	{
		var innerSink = LoggerDiagnosticMessageSink.TryCreate(logger, diagnosticMessages, internalDiagnosticMessages);

		return
			innerSink is not null
				? (IMessageSink)new OutputDeviceDiagnosticMessageSink(outputDevice, diagnosticMessages, internalDiagnosticMessages, innerSink)
				: null;
	}
}
