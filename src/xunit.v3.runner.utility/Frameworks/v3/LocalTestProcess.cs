using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Xunit.v3;

/// <summary>
/// Implementation of <see cref="ITestProcess"/> for a process running on the local machine.
/// </summary>
public sealed class LocalTestProcess : ITestProcess
{
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
			new LocalTestProcess(Process.GetProcessById(processID), responseFile);

	/// <inheritdoc/>
	public void Dispose()
	{
		try
		{
			if (!process.HasExited)
			{
				// We'll start by giving it 15 seconds to finish on its own
				var stopWait = DateTimeOffset.UtcNow.AddSeconds(15);
				while (!process.HasExited && DateTimeOffset.UtcNow < stopWait)
					Thread.Sleep(50);

				// If the sleep wait didn't do it, simulate Ctrl+C to abort
				if (!process.HasExited)
				{
					process.StandardInput.WriteLine("\x3");

					// Give it another 45 seconds to clean itself up, and then
					// just kill it if it never finishes.
					if (!process.WaitForExit(45_000))
						process.Kill();
				}
			}
		}
		catch { }

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
		if (process is null)
			return null;

		return new LocalTestProcess(process, responseFile);
	}

	/// <inheritdoc/>
	public bool WaitForExit(int milliseconds) =>
		process.WaitForExit(milliseconds);
}
