using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Runner.v3;

/// <summary>
/// This class be used to do discovery and execution of xUnit.net v3 tests.
/// </summary>
public class Xunit3 : IFrontController
{
	static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

	readonly IMessageSink? diagnosticMessageSink;
	readonly XunitProjectAssembly projectAssembly;
	readonly ISourceInformationProvider? sourceInformationProvider;
	readonly Func<IReadOnlyList<string>, ITestProcess?> testProcessLauncher;

	Xunit3(
		XunitProjectAssembly projectAssembly,
		ISourceInformationProvider? sourceInformationProvider,
		IMessageSink? diagnosticMessageSink,
		Func<IReadOnlyList<string>, ITestProcess?> testProcessLauncher)
	{
		this.projectAssembly = projectAssembly;
		this.sourceInformationProvider = sourceInformationProvider;
		this.diagnosticMessageSink = diagnosticMessageSink;
		this.testProcessLauncher = Guard.ArgumentNotNull(testProcessLauncher);

		using var process = testProcessLauncher(["-assemblyInfo"]) ?? throw new InvalidOperationException("Process was null");
		if (!process.WaitForExit(60_000))
			throw new InvalidOperationException("Test process did not respond within 60 seconds");

		var output = process.StandardOutput.ReadToEnd().Trim(' ', '\r', '\n');
		if (!JsonDeserializer.TryDeserialize(output, out var json))
			throw new InvalidOperationException("Test process terminated unexpectedly." + (output.Length > 0 ? (" Output:" + Environment.NewLine + output) : string.Empty));
		if (json is not Dictionary<string, object?> root)
			throw new InvalidOperationException("Test process did not return valid JSON (non-object). Output:" + Environment.NewLine + output);
		if (!root.TryGetValue("core-framework", out var coreFrameworkObject) || coreFrameworkObject is not string coreFrameworkString || !Version.TryParse(coreFrameworkString, out var coreFramework))
			throw new InvalidOperationException("Test process did not return valid JSON ('core-framework' is missing or malformed). Output:" + Environment.NewLine + output);
		if (!root.TryGetValue("core-framework-informational", out var coreFrameworkInformationalObject) || coreFrameworkInformationalObject is not string coreFrameworkInformational)
			throw new InvalidOperationException("Test process did not return valid JSON ('core-framework-informational' is missing). Output:" + Environment.NewLine + output);
		if (!root.TryGetValue("target-framework", out var targetFrameworkObject) || targetFrameworkObject is not string targetFramework)
			throw new InvalidOperationException("Test process did not return valid JSON ('target-framework' is missing). Output:" + Environment.NewLine + output);
		if (!root.TryGetValue("test-framework", out var testFrameworkObject) || testFrameworkObject is not string testFramework)
			throw new InvalidOperationException("Test process did not return valid JSON ('test-framework' is missing). Output:" + Environment.NewLine + output);

		CoreFrameworkVersion = coreFramework;
		CoreFrameworkVersionInformational = coreFrameworkInformational;
		TargetFramework = targetFramework;
		TestFrameworkDisplayName = testFramework;
		TestAssemblyUniqueID = UniqueIDGenerator.ForAssembly(projectAssembly.AssemblyFileName, projectAssembly.ConfigFileName);
	}

	/// <inheritdoc/>
	public bool CanUseAppDomains => false;

	/// <summary>
	/// Gets the version of <c>xunit.v3.core.dll</c> the test assembly is linked against.
	/// </summary>
	public Version CoreFrameworkVersion { get; }

	/// <summary>
	/// Gets the informational version of <c>xunit.v3.core.dll</c> the test assembly
	/// is linked against.
	/// </summary>
	public string CoreFrameworkVersionInformational { get; }

	/// <inheritdoc/>
	public string TargetFramework { get; }

	/// <inheritdoc/>
	public string TestAssemblyUniqueID { get; }

	/// <inheritdoc/>
	public string TestFrameworkDisplayName { get; }

	/// <inheritdoc/>
	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		return default;
	}

	/// <inheritdoc/>
	public int? Find(
		IMessageSink messageSink,
		FrontControllerFindSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		var arguments = Xunit3ArgumentFactory.ForFind(
			settings.Options,
			settings.Filters,
			projectAssembly.ConfigFileName,
			ListOption.Discovery,
			settings.LaunchOptions.WaitForDebugger
		);

		var process =
			testProcessLauncher(arguments)
				?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Could not launch test process. Test assembly '{0}', arguments: '{1}'", projectAssembly.AssemblyFileName, string.Join(" ", arguments)));

		// The '-list discovery' only sends test cases, not starting & complete messages,
		// so we'll fabricate them ourselves.
		bool SendDiscoveryStarting(string assemblyUniqueID) =>
			messageSink.OnMessage(new DiscoveryStarting
			{
				AssemblyName = projectAssembly.AssemblyDisplayName,
				AssemblyPath = projectAssembly.AssemblyFileName,
				AssemblyUniqueID = assemblyUniqueID,
				ConfigFilePath = projectAssembly.ConfigFileName,
			});

		ThreadPool.QueueUserWorkItem(async _ =>
		{
			var assemblyUniqueID = default(string);
			var testCaseCount = 0;
			var collectSourceInformation = settings.Options.GetIncludeSourceInformationOrDefault();

			try
			{
				while (true)
				{
					var line = await process.StandardOutput.ReadLineAsync();
					if (line is null)
						break;

					var message = MessageSinkMessageDeserializer.Deserialize(line, diagnosticMessageSink);
					if (message is null)
					{
						diagnosticMessageSink?.OnMessage(new InternalDiagnosticMessage("Received unparseable output from test process: " + line));
						continue;
					}

					if (message is ITestCaseDiscovered testDiscovered)
					{
						// Don't overwrite the source information if it came directly from the test framework
						if (collectSourceInformation && sourceInformationProvider is not null && testDiscovered.SourceFilePath is null && testDiscovered.SourceLineNumber is null)
						{
							var (sourceFile, sourceLine) = sourceInformationProvider.GetSourceInformation(testDiscovered.TestClassName, testDiscovered.TestMethodName);
							testDiscovered = testDiscovered.WithSourceInfo(sourceFile, sourceLine);
						}

						if (assemblyUniqueID is null)
						{
							assemblyUniqueID = testDiscovered.AssemblyUniqueID;
							SendDiscoveryStarting(assemblyUniqueID);
						}

						++testCaseCount;
						if (!messageSink.OnMessage(testDiscovered))
							break;
					}
				}
			}
			finally
			{
				// Dispose first so we don't race against anything waiting for discovery complete.
				// We want the process to be fully cleaned up before runners move on.
				process.Dispose();

				// If we didn't see any test cases, we can compute a unique ID. We also need to make
				// sure we send the starting message before the complete message in this case.
				if (assemblyUniqueID is null)
				{
					assemblyUniqueID = UniqueIDGenerator.ForAssembly(projectAssembly.AssemblyFileName, projectAssembly.ConfigFileName);
					SendDiscoveryStarting(assemblyUniqueID);
				}

				messageSink.OnMessage(new DiscoveryComplete
				{
					AssemblyUniqueID = assemblyUniqueID,
					TestCasesToRun = testCaseCount,
				});
			}
		});

		return process.ID;
	}

	/// <inheritdoc/>
	public int? FindAndRun(
		IMessageSink messageSink,
		FrontControllerFindAndRunSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		var arguments = Xunit3ArgumentFactory.ForFindAndRun(
			settings.DiscoveryOptions,
			settings.ExecutionOptions,
			settings.Filters,
			projectAssembly.ConfigFileName,
			settings.LaunchOptions.WaitForDebugger
		);

		return RunInternal(messageSink, arguments);
	}

	/// <inheritdoc/>
	public int? Run(
		IMessageSink messageSink,
		FrontControllerRunSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);
		Guard.ArgumentNotNullOrEmpty(settings.SerializedTestCases);

		var arguments = Xunit3ArgumentFactory.ForRun(
			settings.Options,
			settings.SerializedTestCases,
			projectAssembly.ConfigFileName,
			settings.LaunchOptions.WaitForDebugger
		);

		return RunInternal(messageSink, arguments);
	}

	int? RunInternal(
		IMessageSink messageSink,
		List<string> arguments)
	{
		var process =
			testProcessLauncher(arguments)
				?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Could not launch test process. Test assembly '{0}', arguments: '{1}'", projectAssembly.AssemblyFileName, string.Join(" ", arguments)));

		ThreadPool.QueueUserWorkItem(async _ =>
		{
			try
			{
				while (true)
				{
					var line = await process.StandardOutput.ReadLineAsync();
					if (line is null)
						break;

					var message = MessageSinkMessageDeserializer.Deserialize(line, diagnosticMessageSink);
					if (message is null)
					{
						diagnosticMessageSink?.OnMessage(new DiagnosticMessage("Received unparseable output from test process: " + line));
						continue;
					}

					if (!messageSink.OnMessage(message))
						break;

					if (message is ITestAssemblyFinished)
						break;
				}
			}
			finally
			{
				process.Dispose();
			}
		});

		return process.ID;
	}

	// Factory method

	/// <summary>
	/// Returns an implementation of <see cref="IFrontController"/> which can be used
	/// for both discovery and execution of xUnit.net v2 tests.
	/// </summary>
	/// <param name="projectAssembly">The test project assembly.</param>
	/// <param name="sourceInformationProvider">The optional source information provider.</param>
	/// <param name="diagnosticMessageSink">The message sink which receives <see cref="IDiagnosticMessage"/>
	/// and <see cref="IInternalDiagnosticMessage"/> messages.</param>
	/// <param name="forceInProcess">A flag to force the assembly to load in-process rather than use out-of-process
	/// execution. Note: this flag requires extensive assembly resolution support by the runner author.</param>
	public static IFrontController ForDiscoveryAndExecution(
		XunitProjectAssembly projectAssembly,
		ISourceInformationProvider? sourceInformationProvider = null,
		IMessageSink? diagnosticMessageSink = null,
		bool forceInProcess = false)
	{
		Guard.ArgumentNotNull(projectAssembly);
		Guard.FileExists(projectAssembly.AssemblyFileName);

		ITestProcess? StartOutOfProcessTestAssembly(IReadOnlyList<string> responseFileArguments)
		{
			if (projectAssembly.AssemblyFileName is null)
				return null;
			if (projectAssembly.AssemblyMetadata is null || projectAssembly.AssemblyMetadata.TargetFrameworkIdentifier == TargetFrameworkIdentifier.UnknownTargetFramework)
				return null;

			if (projectAssembly.AssemblyMetadata.TargetFrameworkIdentifier == TargetFrameworkIdentifier.DotNetCore)
			{
				// We want to run the executable stub rather than 'dotnet exec' because it will seek out the appropriate
				// bitness of the .NET SDK when appropriate.
				var executable = projectAssembly.AssemblyFileName;

				// We always expect them to pass .dll here, because that's the actual test assembly. The executable stub won't
				// have appropriate metadata, but we'll just be safe here anyways.
				if (executable.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
					executable = executable.Substring(0, projectAssembly.AssemblyFileName.Length - 4);
				if (IsWindows)
					executable += ".exe";

				return OutOfProcessTestProcess.Create(executable, string.Empty, responseFileArguments);
			}
			else if (!IsWindows)
				return OutOfProcessTestProcess.Create("mono", string.Format(CultureInfo.InvariantCulture, "\"{0}\"", projectAssembly.AssemblyFileName), responseFileArguments);
			else
				return OutOfProcessTestProcess.Create(projectAssembly.AssemblyFileName, string.Empty, responseFileArguments);
		}

		ITestProcess? StartInProcessTestAssembly(IReadOnlyList<string> responseFileArguments)
		{
			if (projectAssembly.AssemblyFileName is null)
				return null;
			if (projectAssembly.AssemblyMetadata is null || projectAssembly.AssemblyMetadata.TargetFrameworkIdentifier == TargetFrameworkIdentifier.UnknownTargetFramework)
				return null;

			// TODO: Should we validate that we match target frameworks?

			return InProcessTestProcess.Create(projectAssembly.AssemblyFileName, responseFileArguments);
		}

		return new Xunit3(projectAssembly, sourceInformationProvider, diagnosticMessageSink, forceInProcess ? StartInProcessTestAssembly : StartOutOfProcessTestAssembly);
	}

	sealed internal class OutOfProcessTestProcess : ITestProcess
	{
		readonly Process process;
		readonly string? responseFile;

		OutOfProcessTestProcess(
			Process process,
			string? responseFile)
		{
			this.process = process;
			this.responseFile = responseFile;
		}

		public int? ID =>
			process.Id;

		public TextReader StandardOutput =>
			process.StandardOutput;

		public static ITestProcess? Create(
			string executable,
			string executableArguments,
			IReadOnlyList<string> responseFileArguments)
		{
			string? responseFile = default;

			if (responseFileArguments.Count != 0)
			{
				responseFile = Path.GetTempFileName();
				File.WriteAllLines(responseFile, responseFileArguments);

				if (executableArguments.Length != 0)
					executableArguments += " ";

				executableArguments += string.Format(CultureInfo.InvariantCulture, "@@ \"{0}\"", responseFile);
			}

			var psi = new ProcessStartInfo(executable, executableArguments)
			{
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
			};

			var process = Process.Start(psi);
			if (process is null)
				return null;

			return new OutOfProcessTestProcess(process, responseFile);
		}

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

		public bool WaitForExit(int milliseconds) => process.WaitForExit(milliseconds);
	}

	internal sealed class InProcessTestProcess : ITestProcess
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

		public int? ID => null;

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
				var consoleRunner = Activator.CreateInstance(consoleRunnerType, [executableArguments.ToArray()]);
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
