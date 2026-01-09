using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// These are the options used when creating <see cref="ExecutionSink"/>. This is
/// set up as an options class so that new options can be added without breaking
/// binary compatibility.
/// </summary>
public class ExecutionSinkOptions
{
	/// <summary>
	/// This value is no longer used by ExecutionSink, and will be removed in the next major version.
	/// </summary>
	[Obsolete("This value is no longer used by ExecutionSink, and will be removed in the next major version.")]
	public XElement? AssemblyElement { get; set; }

	/// <summary>
	/// Gets or sets a thunk to be used to determine whether cancellation has been requested.
	/// </summary>
	public Func<bool>? CancelThunk { get; set; }

	/// <summary>
	/// Gets or sets the diagnostic message sink to report diagnostic messages to. In order
	/// for long running tests to be reported, this must not be <see langword="null"/>.
	/// </summary>
	public IMessageSink? DiagnosticMessageSink { get; set; }

	/// <summary>
	/// Gets or sets a callback to be called when execution is complete.
	/// </summary>
	public Action<ExecutionSummary>? FinishedCallback { get; set; }

	/// <summary>
	/// Gets or sets a flag indicating whether skipped tests should be reported as failed
	/// tests. If this is not <see langword="true"/>, then skipped tests will be reported as skipped.
	/// </summary>
	public bool FailSkips { get; set; }

	/// <summary>
	/// Gets or sets a flag indicating whether passing tests with warnings should be
	/// reported as failed tests. If this is not <see langword="true"/>, then passing tests with
	/// warnings will be reported as passing tests.
	/// </summary>
	public bool FailWarn { get; set; }

	/// <summary>
	/// Gets or sets a callback to be called when a long running test has been detected.
	/// </summary>
	public Action<LongRunningTestsSummary>? LongRunningTestCallback { get; set; }

	/// <summary>
	/// Gets or sets the time after which to report long running tests. If the time span
	/// specified here is not greater than <see cref="TimeSpan.Zero"/>, then long running
	/// tests will not be detected.
	/// </summary>
	public TimeSpan LongRunningTestTime { get; set; }

	/// <summary>
	/// Gets or sets the list of result writer message handlers that will be supported by the
	/// execution sink.
	/// </summary>
	public IReadOnlyCollection<IResultWriterMessageHandler>? ResultWriterMessageHandlers { get; set; }
}
