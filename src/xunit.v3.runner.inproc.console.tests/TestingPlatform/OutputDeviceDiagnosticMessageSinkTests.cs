using System;
using System.Collections.Generic;
using Microsoft.Testing.Platform.OutputDevice;
using Xunit;
using Xunit.Runner.InProc.SystemConsole.TestingPlatform;
using Xunit.Sdk;

public class OutputDeviceDiagnosticMessageSinkTests
{
	public class OnMessage
	{
		public static IEnumerable<TheoryDataRow<IMessageSinkMessage, string?, ConsoleColor?>> MessageData =
		[
			new(
				TestData.DiagnosticMessage("Hello from diagnostics"),
				"Hello from diagnostics",
				ConsoleColor.Yellow
			),
			new(
				TestData.InternalDiagnosticMessage("Hello from internal diagnostics"),
				"Hello from internal diagnostics",
				ConsoleColor.DarkGray
			),
			new(
				TestData.TestCaseDiscovered(),
				null,
				null
			),
		];

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(MessageData))]
		public void ForwardsDiagnosticMessages(
			IMessageSinkMessage message,
			string? expectedLog,
			ConsoleColor? expectedColor)
		{
			var outputDevice = new SpyTestPlatformOutputDevice();
			var innerSink = SpyMessageSink.Capture();
			var sink = new OutputDeviceDiagnosticMessageSink(outputDevice, diagnosticMessages: true, internalDiagnosticMessages: true, innerSink);

			sink.OnMessage(message);

			var innerSinkMessage = Assert.Single(innerSink.Messages);
			Assert.Same(message, innerSinkMessage);
			if (expectedLog is null)
				Assert.Empty(outputDevice.DisplayedData);
			else
			{
				var datum = Assert.Single(outputDevice.DisplayedData);
				var formattedDatum = Assert.IsType<FormattedTextOutputDeviceData>(datum);
				var color = Assert.IsType<SystemConsoleColor>(formattedDatum.ForegroundColor);
				Assert.Equal(expectedColor, color.ConsoleColor);
				Assert.Equal(expectedLog, formattedDatum.Text);
			}
		}

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(MessageData))]
		public void DoesNotForwardMessagesWhenDisabled(
			IMessageSinkMessage message,
			string? _1,
			ConsoleColor? _2)
		{
			var outputDevice = new SpyTestPlatformOutputDevice();
			var sink = new OutputDeviceDiagnosticMessageSink(outputDevice, diagnosticMessages: false, internalDiagnosticMessages: false, SpyMessageSink.Create());

			sink.OnMessage(message);

			Assert.Empty(outputDevice.DisplayedData);
		}
	}

	public class TryCreate
	{
		[Theory]
		[InlineData(false, false, false)]
		[InlineData(false, true, true)]
		[InlineData(true, false, true)]
		[InlineData(true, true, true)]
		public void Creation(
			bool diagnosticMessages,
			bool internalDiagnosticMessages,
			bool objectShouldBeCreated)
		{
			var logger = new SpyTestPlatformLogger();
			var outputDevice = new SpyTestPlatformOutputDevice();

			var sink = OutputDeviceDiagnosticMessageSink.TryCreate(logger, outputDevice, diagnosticMessages, internalDiagnosticMessages);

			if (objectShouldBeCreated)
			{
				var odms = Assert.IsType<OutputDeviceDiagnosticMessageSink>(sink);
				Assert.IsType<LoggerDiagnosticMessageSink>(odms.InnerSink);
			}
			else
				Assert.Null(sink);
		}
	}
}
