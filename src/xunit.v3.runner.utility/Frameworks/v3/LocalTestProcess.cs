using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Xunit.v3;

/// <summary>
/// Implementation of <see cref="ITestProcess"/> for a process running on the local machine.
/// </summary>
public sealed class LocalTestProcess : ITestProcess
{
	volatile int cancelSent;
	readonly Process process;
	readonly string? responseFile;

	LocalTestProcess(
		Process process,
		string? responseFile)
	{
		this.process = process;
		this.responseFile = responseFile;
	}

	/// <inheritdoc/>
	public bool HasExited =>
		process.HasExited;

	/// <summary>
	/// Gets the process ID of the local process.
	/// </summary>
	public int ProcessID =>
		process.Id;

	/// <inheritdoc/>
	public TextWriter StandardInput =>
		process.StandardInput;

	/// <inheritdoc/>
	public TextReader StandardOutput =>
		process.StandardOutput;

	/// <summary>
	/// Returns a test process for an existing process based on process ID.
	/// </summary>
	/// <param name="processID">The process ID</param>
	/// <param name="responseFile">The response file (to be cleaned up during disposal)</param>
	public static LocalTestProcess Attach(
		int processID,
		string? responseFile) =>
			new(Process.GetProcessById(processID), responseFile);

	/// <inheritdoc/>
	public void Cancel(bool forceCancellation)
	{
		try
		{
			if (forceCancellation)
			{
				if (!process.HasExited)
				{
					// Make sure we sent the first Ctrl+C, then give it 15 seconds to finish up. If it doesn't
					// finish at that point, then just terminate the process.
					Cancel(false);
					if (!process.WaitForExit(15_000))
						process.Kill();
				}
			}
			else
			{
				if (Interlocked.Exchange(ref cancelSent, 1) == 0)
					process.StandardInput.Write('\x03');
			}
		}
		catch { }
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		Cancel(forceCancellation: true);

		try
		{
			if (responseFile is not null)
				File.Delete(responseFile);
		}
		catch { }
	}

	/// <summary>
	/// Starts a new test process.
	/// </summary>
	/// <param name="executable">The executable to be launched</param>
	/// <param name="executableArguments">The arguments to the executable</param>
	/// <param name="responseFile">The response file (to be cleaned up during disposal)</param>
	public static LocalTestProcess? Start(
		string executable,
		string executableArguments,
		string? responseFile)
	{
		var psi = new ProcessStartInfo(executable, executableArguments)
		{
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			UseShellExecute = false,
		};

		var process = Process.Start(psi);

		return
			process is not null
				? new LocalTestProcess(process, responseFile)
				: null;
	}

	/// <inheritdoc/>
	public bool WaitForExit(int milliseconds) =>
		process.WaitForExit(milliseconds);
}
