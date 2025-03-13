using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Sdk;

// These tests do not instantiate TraceAssertOverrideListener; they simply verify that it's
// in use by default. It is instantiated very early in the lifecycle of ConsoleRunner.EntryPoint
// or TestPlatformTestFramework.RunAsync in xunit.v3.runner.inproc.console

public class TraceAssertOverrideListenerTests
{
	public static IEnumerable<TheoryDataRow<string?, string?, string>> MessageData = [
		new(null, null, "Trace/Debug.Assert() Failure"),
		new("message", null, "Trace/Debug.Assert() Failure: message"),
		new(null, "details", "Trace/Debug.Assert() Failure: details"),
		new("message", "details", "Trace/Debug.Assert() Failure: message" + Environment.NewLine + "details"),
	];

	[Theory]
	[MemberData(nameof(MessageData))]
	public void ConvertsTraceAssertIntoFailureException(
		string? message,
		string? detailMessage,
		string exceptedAssertMessage)
	{
		var ex = Record.Exception(() => Trace.Assert(false, message, detailMessage));

		Assert.IsType<TraceAssertException>(ex);
		Assert.Equal(exceptedAssertMessage, ex.Message);
	}

#if DEBUG

	[Theory]
	[MemberData(nameof(MessageData))]
	public void ConvertsDebugAssertIntoFailureException(
		string? message,
		string? detailMessage,
		string exceptedAssertMessage)
	{
		var ex = Record.Exception(() => Debug.Assert(false, message, detailMessage));

		Assert.IsType<TraceAssertException>(ex);
		Assert.Equal(exceptedAssertMessage, ex.Message);
	}

#else

	[Theory]
	[MemberData(nameof(MessageData))]
	public void DoesNotConvertDebugAssertIntoFailureException(
		string? message,
		string? detailMessage,
		string _)
	{
		var ex = Record.Exception(() => Debug.Assert(false, message, detailMessage));

		Assert.Null(ex);
	}

#endif
}
