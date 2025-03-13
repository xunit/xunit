using System.Diagnostics;
using System.Linq;
using Xunit.Sdk;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public class TraceAssertOverrideListener : TraceListener
{
	readonly bool assertUiEnabled;
#pragma warning disable CA2213  // We're keeping this copy to prevent looking it up twice, not to own it
	readonly DefaultTraceListener? defaultTraceListener;
#pragma warning restore CA2213

	/// <summary/>
	public TraceAssertOverrideListener()
	{
		// Need to disable the default trace listener's display of UI, which is what causes the
		// process termination.
		defaultTraceListener = Trace.Listeners.OfType<DefaultTraceListener>().FirstOrDefault();
		if (defaultTraceListener is not null)
		{
			assertUiEnabled = defaultTraceListener.AssertUiEnabled;
			defaultTraceListener.AssertUiEnabled = false;
		}

		Trace.Listeners.Add(this);
	}

	/// <inheritdoc/>
	protected override void Dispose(bool disposing)
	{
		Trace.Listeners.Remove(this);

		if (defaultTraceListener is not null)
			defaultTraceListener.AssertUiEnabled = assertUiEnabled;

		base.Dispose(disposing);
	}

	/// <summary/>
	public override void Fail(string? message) =>
		throw TraceAssertException.ForAssertFailure(message, null);

	/// <summary/>
	public override void Fail(
		string? message,
		string? detailMessage) =>
			throw TraceAssertException.ForAssertFailure(message, detailMessage);

	/// <summary/>
	public override void Write(string? message)
	{ }

	/// <summary/>
	public override void WriteLine(string? message)
	{ }
}
