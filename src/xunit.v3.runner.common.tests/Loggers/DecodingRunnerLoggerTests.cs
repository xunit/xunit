using System;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;

public class DecodingRunnerLoggerTests
{
	[Fact]
	public void DiagnosticMessage()
	{
		var spySink = SpyMessageSink.Capture();
		var spyDiagnosticSink = SpyMessageSink.Capture();
		var json = new DiagnosticMessage("message").ToJson();
		var logger = new DecodingRunnerLogger(spySink, spyDiagnosticSink);

		logger.LogMessage(json);

		Assert.Empty(spySink.Messages);
		var message = Assert.IsAssignableFrom<IDiagnosticMessage>(Assert.Single(spyDiagnosticSink.Messages));
		Assert.Equal("message", message.Message);
	}

	[Fact]
	public void InternalDiagnosticMessage()
	{
		var spySink = SpyMessageSink.Capture();
		var spyDiagnosticSink = SpyMessageSink.Capture();
		var json = new InternalDiagnosticMessage("message").ToJson();
		var logger = new DecodingRunnerLogger(spySink, spyDiagnosticSink);

		logger.LogMessage(json);

		Assert.Empty(spySink.Messages);
		var message = Assert.IsAssignableFrom<IInternalDiagnosticMessage>(Assert.Single(spyDiagnosticSink.Messages));
		Assert.Equal("message", message.Message);
	}

	[Fact]
	public void StandardMessage()
	{
		var spySink = SpyMessageSink.Capture();
		var spyDiagnosticSink = SpyMessageSink.Capture();
		var json = new DiscoveryComplete { AssemblyUniqueID = "asm-id", TestCasesToRun = 42 }.ToJson();
		var logger = new DecodingRunnerLogger(spySink, spyDiagnosticSink);

		logger.LogMessage(json);

		var message = Assert.IsAssignableFrom<IDiscoveryComplete>(Assert.Single(spySink.Messages));
		Assert.Equal("asm-id", message.AssemblyUniqueID);
		Assert.Equal(42, message.TestCasesToRun);
		Assert.Empty(spyDiagnosticSink.Messages);
	}

	public static TheoryData<string, string> NonMessageData =
	[
		("{}", $"JSON message deserialization failure: root object did not include string property '$type'{Environment.NewLine}{{}}"),
		("{\"$type\":\"foo\"}", "JSON message deserialization failure: message '$type' foo does not have an associated registration"),
		("Hello world", $"JSON message deserialization failure: invalid JSON, or not a JSON object{Environment.NewLine}Hello world"),
	];

	[Theory]
	[MemberData(nameof(NonMessageData))]
	public void NonMessagesReportInternalDiagnostics(
		string json,
		string expectedDiagnostic)
	{
		var spySink = SpyMessageSink.Capture();
		var spyDiagnosticSink = SpyMessageSink.Capture();
		var logger = new DecodingRunnerLogger(spySink, spyDiagnosticSink);

		logger.LogMessage(json);

		Assert.Empty(spySink.Messages);
		var message = Assert.IsAssignableFrom<IInternalDiagnosticMessage>(Assert.Single(spyDiagnosticSink.Messages));
		Assert.Equal<object>(expectedDiagnostic, message.Message);
	}
}
