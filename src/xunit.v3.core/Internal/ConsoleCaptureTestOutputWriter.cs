using System;
using System.IO;
using System.Text;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public class ConsoleCaptureTestOutputWriter : TextWriter
{
	readonly TextWriter? consoleError;
	readonly TextWriter? consoleOut;
	readonly ITestContextAccessor testContextAccessor;

	/// <summary/>
	public ConsoleCaptureTestOutputWriter(
		ITestContextAccessor testContextAccessor,
		bool captureError,
		bool captureOut)
	{
		this.testContextAccessor = testContextAccessor;

		if (captureOut)
		{
			consoleOut = Console.Out;
			Console.SetOut(this);
		}

		if (captureError)
		{
			consoleError = Console.Error;
			Console.SetError(this);
		}
	}

	/// <inheritdoc/>
	public override Encoding Encoding =>
		Encoding.Unicode;

	/// <inheritdoc/>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (consoleOut is not null)
				Console.SetOut(consoleOut);

			if (consoleError is not null)
				Console.SetError(consoleError);
		}

		base.Dispose(disposing);
	}

	/// <inheritdoc/>
	public override void Write(char value) =>
		testContextAccessor.Current.TestOutputHelper?.Write(new string(value, 1));

	/// <inheritdoc/>
	public override void Write(char[] buffer, int index, int count) =>
		testContextAccessor.Current.TestOutputHelper?.Write(new string(buffer, index, count));

	/// <inheritdoc/>
	public override void Write(string? value)
	{
		if (value is not null)
			testContextAccessor.Current.TestOutputHelper?.Write(value);
	}
}
