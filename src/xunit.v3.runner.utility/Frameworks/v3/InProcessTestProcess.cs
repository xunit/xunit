using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.v3;

/// <summary>
/// The <see cref="ITestProcess"/> implementation used by <see cref="InProcessTestProcessLauncher"/>.
/// </summary>
/// <remarks>
/// Rather than launching the process externally, we load the test assembly via <see cref="Assembly.LoadFrom(string)"/>
/// and expect that the runner author has already taken care of any dependency resolution that's necessary for such
/// a load to succeed. We dynamically create the instance of <c>Xunit.Runner.InProc.SystemConsole.ConsoleRunner</c> and
/// call its <c>EntryPoint</c> and <c>Cancel</c> methods as appropriate.
/// </remarks>
internal sealed class InProcessTestProcess :
	ITestProcess
{
	readonly Action cancelMethod;
	readonly string? responseFile;
	readonly BufferedTextReaderWriter stdIn;
	readonly BufferedTextReaderWriter stdOut;
	readonly Thread workerThread;

	InProcessTestProcess(
		Action cancelMethod,
		BufferedTextReaderWriter stdIn,
		BufferedTextReaderWriter stdOut,
		string? responseFile,
		Thread workerThread)
	{
		this.cancelMethod = cancelMethod;
		this.stdIn = stdIn;
		this.stdOut = stdOut;
		this.responseFile = responseFile;
		this.workerThread = workerThread;
	}

	public bool HasExited =>
		(workerThread.ThreadState & ThreadState.Stopped) == ThreadState.Stopped;

	public TextWriter StandardInput =>
		stdIn.Writer;

	public TextReader StandardOutput =>
		stdOut.Reader;

	public void Cancel(bool forceCancellation)
	{
		if (HasExited)
			return;

		cancelMethod();
	}

	public static ITestProcess? Create(
		string testAssembly,
		IReadOnlyList<string> responseFileArguments)
	{
		var assembly = Assembly.LoadFrom(testAssembly);
		var inprocRunnerAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "xunit.v3.runner.inproc.console");
		if (inprocRunnerAssembly is null)
			return null;

		var consoleRunnerType = inprocRunnerAssembly.GetType("Xunit.Runner.InProc.SystemConsole.ConsoleRunner");
		if (consoleRunnerType is null)
			return null;

		var entryPointMethod = consoleRunnerType.GetMethod("EntryPoint", [typeof(TextReader), typeof(TextWriter)]);
		if (entryPointMethod is null)
			return null;

		var cancelMethod = consoleRunnerType.GetMethod("Cancel", []);
		if (cancelMethod is null)
			return null;

		string? responseFile = default;
		List<string> executableArguments = [];

		if (responseFileArguments.Count != 0)
		{
			responseFile = Path.GetTempFileName();
			File.WriteAllLines(responseFile, responseFileArguments);

			executableArguments.Add("@@");
			executableArguments.Add(responseFile);
		}

		try
		{
			var stdIn = new BufferedTextReaderWriter();
			var stdOut = new BufferedTextReaderWriter();
			var consoleRunner = Activator.CreateInstance(consoleRunnerType, [executableArguments.ToArray(), assembly]);
			var workerThread = new Thread(() =>
			{
				using var stdInReader = stdIn.Reader;
				using var stdOutWriter = stdOut.Writer;
				var task = entryPointMethod.Invoke(consoleRunner, [stdInReader, stdOutWriter]) as Task<int>;
				task?.GetAwaiter().GetResult();
			});
			workerThread.Start();

			return new InProcessTestProcess(() => cancelMethod.Invoke(consoleRunner, []), stdIn, stdOut, responseFile, workerThread);
		}
		catch (Exception)
		{
			try
			{
				if (responseFile is not null)
					File.Delete(responseFile);
			}
			catch { }

			throw;
		}
	}

	public void Dispose()
	{
		try
		{
			// We can't forcefully close an in-process test run, because Thread.Abort was removed from .NET
			// so we'll request graceful termination and give it 15 seconds to finish.
			Cancel(false);
			WaitForExit(15_000);
		}
		catch { }

		try
		{
			if (responseFile is not null)
				File.Delete(responseFile);
		}
		catch { }
	}

	public bool WaitForExit(int milliseconds) =>
		workerThread.Join(milliseconds);
}
