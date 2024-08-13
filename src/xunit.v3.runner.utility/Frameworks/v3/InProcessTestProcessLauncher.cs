using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Runner.Common;

namespace Xunit.v3;

/// <summary>
/// Implementation of <see cref="ITestProcessLauncher"/> that will launch an xUnit.net v3 test
/// in-process.
/// </summary>
/// <remarks>
/// Note that this will require the runner author to implement dependency resolution, as no attempt
/// to do so is done here.
/// </remarks>
public sealed class InProcessTestProcessLauncher : ITestProcessLauncher
{
	InProcessTestProcessLauncher()
	{ }

	/// <summary>
	/// Gets the singleton instance of <see cref="InProcessTestProcessLauncher"/>.
	/// </summary>
	public static InProcessTestProcessLauncher Instance { get; } = new();

	/// <inheritdoc/>
	public ITestProcess? Launch(
		XunitProjectAssembly projectAssembly,
		IReadOnlyList<string> arguments)
	{
		Guard.ArgumentNotNull(projectAssembly);
		Guard.ArgumentNotNull(arguments);

		if (projectAssembly.AssemblyFileName is null)
			return default;
		if (projectAssembly.AssemblyMetadata is null || projectAssembly.AssemblyMetadata.TargetFrameworkIdentifier == TargetFrameworkIdentifier.UnknownTargetFramework)
			return default;

		// TODO: Should we validate that we match target frameworks?

		return InProcessTestProcess.Create(projectAssembly.AssemblyFileName, arguments);
	}

	sealed class InProcessTestProcess : ITestProcess
	{
		readonly Action cancelMethod;
		readonly BufferedTextReaderWriter readerWriter;
		readonly string? responseFile;
		readonly Thread workerThread;

		InProcessTestProcess(
			Action cancelMethod,
			BufferedTextReaderWriter readerWriter,
			string? responseFile,
			Thread workerThread)
		{
			this.cancelMethod = cancelMethod;
			this.readerWriter = readerWriter;
			this.responseFile = responseFile;
			this.workerThread = workerThread;
		}

		public bool HasExited => throw new NotImplementedException();

		public TextReader StandardOutput => readerWriter.Reader;

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

			var entryPointMethod = consoleRunnerType.GetMethod("EntryPoint", [typeof(TextWriter)]);
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
				var bufferedReaderWriter = new BufferedTextReaderWriter();
				var consoleRunner = Activator.CreateInstance(consoleRunnerType, [executableArguments.ToArray(), assembly]);
				var workerThread = new Thread(async () =>
				{
					using var writer = bufferedReaderWriter.Writer;
					var task = entryPointMethod.Invoke(consoleRunner, [writer]) as Task<int>;
					if (task is not null)
						await task;
				});
				workerThread.Start();

				return new InProcessTestProcess(() => cancelMethod.Invoke(consoleRunner, []), bufferedReaderWriter, responseFile, workerThread);
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
				cancelMethod();

				// Give the worker thread 15 seconds to finish on its own. There's nothing else we
				// can do if it doesn't finish, because Thread.Abort is not supported in .NET Core.
				workerThread.Join(15_000);
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

		sealed class BufferedTextReaderWriter
		{
			readonly ConcurrentQueue<char> buffer = new();
			volatile bool closed;

			public BufferedTextReaderWriter()
			{
				Reader = new BufferedReader(this);
				Writer = new BufferedWriter(this);
			}

			public TextReader Reader { get; }

			public TextWriter Writer { get; }

			sealed class BufferedReader(BufferedTextReaderWriter parent) : TextReader
			{
				public override int Peek()
				{
					while (true)
					{
						if (parent.closed && parent.buffer.IsEmpty)
							return -1;

						if (parent.buffer.TryPeek(out var result))
							return result;

						Thread.Sleep(10);
					}
				}

				public override int Read()
				{
					while (true)
					{
						if (parent.closed && parent.buffer.IsEmpty)
							return -1;

						if (parent.buffer.TryDequeue(out var result))
							return result;

						Thread.Sleep(10);
					}
				}
			}

			sealed class BufferedWriter(BufferedTextReaderWriter parent) : TextWriter
			{
				public override Encoding Encoding =>
					Encoding.UTF8;

				protected override void Dispose(bool disposing)
				{
					parent.closed = true;
					base.Dispose(disposing);
				}

				public override void Write(char value) =>
					parent.buffer.Enqueue(value);
			}
		}
	}
}
