using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

public class MessageSplitMessageSinkTests
{
	public static IEnumerable<TheoryDataRow<IMessageSinkMessage>> DiagnosticMessageData =
	[
		new DiagnosticMessage("message"),
		new InternalDiagnosticMessage("message"),
	];

	[Theory(DisableDiscoveryEnumeration = true)]
	[MemberData(nameof(DiagnosticMessageData))]
	public void DiagnosticMessages(IMessageSinkMessage message)
	{
		var spySink = SpyMessageSink.Capture();
		var spyDiagnosticSink = SpyMessageSink.Capture();
		var sink = new MessageSplitMessageSink(spySink, spyDiagnosticSink);

		sink.OnMessage(message);

		Assert.Empty(spySink.Messages);
		Assert.Same(message, Assert.Single(spyDiagnosticSink.Messages));
	}

	public static IEnumerable<TheoryDataRow<IMessageSinkMessage>> NonDiagnosticMessageData =
	[
		new DiscoveryComplete { AssemblyUniqueID = "asm-id", TestCasesToRun = 42 },
		new(ErrorMessage.FromException(new DivideByZeroException())),
	];

	[Theory(DisableDiscoveryEnumeration = true)]
	[MemberData(nameof(NonDiagnosticMessageData))]
	public void NonDiagnosticMessages(IMessageSinkMessage message)
	{
		var spySink = SpyMessageSink.Capture();
		var spyDiagnosticSink = SpyMessageSink.Capture();
		var sink = new MessageSplitMessageSink(spySink, spyDiagnosticSink);
		sink.OnMessage(message);

		Assert.Same(message, Assert.Single(spySink.Messages));
		Assert.Empty(spyDiagnosticSink.Messages);
	}
}
