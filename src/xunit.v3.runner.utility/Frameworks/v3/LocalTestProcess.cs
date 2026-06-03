using System.Diagnostics;

namespace Xunit.v3;

/// <summary>
/// Implementation of <see cref="ITestProcess"/> for a process running on the local machine.
/// </summary>
public sealed class LocalTestProcess : ITestProcess, ITestProcessWithExitCode
{
	volatile int cancelSent;
	readonly Process process;
	readonly string? responseFile;
	readonly int shutdownProcessWaitMilliseconds;

	LocalTestProcess(
		Process process,
		string? responseFile,
		int shutdownProcessWaitSeconds)
	{
		this.process = process;
		this.responseFile = responseFile;

		Guard.ArgumentValid("Value must be greater than 0", shutdownProcessWaitSeconds > 0, nameof(shutdownProcessWaitSeconds));
		shutdownProcessWaitMilliseconds = shutdownProcessWaitSeconds * 1000;
	}

	/// <inheritdoc/>
	public int? ExitCode =>
		process.HasExited ? process.ExitCode : null;

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
	/// <remarks>
	/// This waits for a default of 15 seconds for the process to shut down. Call <see cref="Attach(int, string?, int)"/>
	/// to set a custom length of time to wait.
	/// </remarks>
	public static LocalTestProcess Attach(
		int processID,
		string? responseFile) =>
			Attach(processID, responseFile, 15);

	/// <summary>
	/// Returns a test process for an existing process based on process ID.
	/// </summary>
	/// <param name="processID">The process ID</param>
	/// <param name="responseFile">The response file (to be cleaned up during disposal)</param>
	/// <param name="shutdownProcessWaitSeconds">The number of seconds to wait for the process to shut down
	/// (must be a value greater than zero)</param>
	public static LocalTestProcess Attach(
		int processID,
		string? responseFile,
		int shutdownProcessWaitSeconds) =>
			new(Process.GetProcessById(processID), responseFile, shutdownProcessWaitSeconds);

	/// <inheritdoc/>
	public void Cancel(bool forceCancellation)
	{
		try
		{
			if (forceCancellation)
			{
				if (!process.HasExited)
				{
					// Make sure we sent the first Ctrl+C, then give it time to finish up. If it doesn't
					// finish at that point, then just terminate the process.
					Cancel(false);

					var stopwatch = Stopwatch.StartNew();
					while (true)
					{
						if (stopwatch.ElapsedMilliseconds > shutdownProcessWaitMilliseconds)
						{
							if (!process.HasExited)
								process.Kill();
							break;
						}

						if (process.HasExited && StandardOutput.Peek() == -1)
							break;

						Thread.Yield();
					}
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
	/// <remarks>
	/// This waits for a default of 15 seconds for the process to shut down. Call <see cref="Start(string, string, string?, int)"/>
	/// to set a custom length of time to wait.
	/// </remarks>
	public static LocalTestProcess? Start(
		string executable,
		string executableArguments,
		string? responseFile) =>
			Start(executable, executableArguments, responseFile, 15);

	/// <summary>
	/// Starts a new test process.
	/// </summary>
	/// <param name="executable">The executable to be launched</param>
	/// <param name="executableArguments">The arguments to the executable</param>
	/// <param name="responseFile">The response file (to be cleaned up during disposal)</param>
	/// <param name="shutdownProcessWaitSeconds">The number of seconds to wait for the process to shut down
	/// (must be a value greater than zero)</param>
	public static LocalTestProcess? Start(
		string executable,
		string executableArguments,
		string? responseFile,
		int shutdownProcessWaitSeconds)
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
				? new LocalTestProcess(process, responseFile, shutdownProcessWaitSeconds)
				: null;
	}

	/// <inheritdoc/>
	public bool WaitForExit(int milliseconds) =>
		process.WaitForExit(milliseconds);
}
