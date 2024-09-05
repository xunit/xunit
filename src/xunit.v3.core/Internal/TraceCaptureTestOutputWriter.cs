using System.Diagnostics;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public sealed class TraceCaptureTestOutputWriter : TraceListener
{
	readonly ITestContextAccessor testContextAccessor;

	/// <summary/>
	public TraceCaptureTestOutputWriter(ITestContextAccessor testContextAccessor)
	{
		this.testContextAccessor = testContextAccessor;

		Trace.Listeners.Add(this);
	}

	/// <inheritdoc/>
	protected override void Dispose(bool disposing)
	{
		Trace.Listeners.Remove(this);

		base.Dispose(disposing);
	}

	/// <inheritdoc/>
	public override void Write(string? message)
	{
		if (message is not null)
			testContextAccessor.Current.TestOutputHelper?.Write(message);
	}

	/// <inheritdoc/>
	public override void WriteLine(string? message)
	{
		if (message is not null)
			testContextAccessor.Current.TestOutputHelper?.WriteLine(message);
	}
}
