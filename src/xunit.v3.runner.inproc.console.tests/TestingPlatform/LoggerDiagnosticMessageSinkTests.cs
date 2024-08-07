using System.Collections.Generic;
using Xunit;
using Xunit.Runner.InProc.SystemConsole.TestingPlatform;
using Xunit.Sdk;

public class LoggerDiagnosticMessageSinkTests
{
	public class OnMessage
	{
		public static IEnumerable<TheoryDataRow<IMessageSinkMessage, string?>> MessageData =
		[
			new(
				TestData.DiagnosticMessage("Hello from diagnostics"),
				"[Information] Hello from diagnostics"
			),
			new(
				TestData.InternalDiagnosticMessage("Hello from internal diagnostics"),
				"[Information] Hello from internal diagnostics"
			),
			new(
				TestData.TestCaseDiscovered(),
				null
			),
		];

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(MessageData))]
		public void ForwardsDiagnosticMessages(
			IMessageSinkMessage message,
			string? expectedLog)
		{
			var logger = new SpyTestPlatformLogger();
			var sink = new LoggerDiagnosticMessageSink(logger, diagnosticMessages: true, internalDiagnosticMessages: true);

			sink.OnMessage(message);

			if (expectedLog is null)
				Assert.Empty(logger.Messages);
			else
			{
				var log = Assert.Single(logger.Messages);
				Assert.Equal(expectedLog, log);
			}
		}

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(MessageData))]
		public void DoesNotForwardMessagesWhenDisabled(
			IMessageSinkMessage message,
			string? _)
		{
			var logger = new SpyTestPlatformLogger();
			var sink = new LoggerDiagnosticMessageSink(logger, diagnosticMessages: false, internalDiagnosticMessages: false);

			sink.OnMessage(message);

			Assert.Empty(logger.Messages);
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

			var sink = LoggerDiagnosticMessageSink.TryCreate(logger, diagnosticMessages, internalDiagnosticMessages);

			if (objectShouldBeCreated)
				Assert.IsType<LoggerDiagnosticMessageSink>(sink);
			else
				Assert.Null(sink);
		}
	}
}
