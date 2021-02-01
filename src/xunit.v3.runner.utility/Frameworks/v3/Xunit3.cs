using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

// TODO: Because the engine has asynchronous state, many of the operations here probably need
// to wait for the engine to reach the Connected state before doing the requested operation.
// This includes metadata properties (like TestFrameworkDisplayName) as well as operations
// (like Find and Run).
namespace Xunit.Runner.v3
{
	/// <summary>
	/// This class be used to do discovery and execution of xUnit.net v3 tests.
	/// Runner authors are strongly encouraged to use <see cref="XunitFrontController"/>
	/// instead of using this class directly.
	/// </summary>
	public class Xunit3 : IFrontController, IAsyncDisposable
	{
		readonly _IMessageSink diagnosticMessageSink;
		readonly DisposalTracker disposalTracker = new();
		readonly ConcurrentDictionary<string, _IMessageSink> operations = new();
		readonly XunitProjectAssembly projectAssembly;
		readonly Process process;
		readonly TcpRunnerEngine runnerEngine;
		readonly _ISourceInformationProvider sourceInformationProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit3"/> class.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="projectAssembly">The project assembly to launch for find/run operations.</param>
		/// <param name="sourceInformationProvider">Source code information provider.</param>
		public Xunit3(
			_IMessageSink diagnosticMessageSink,
			XunitProjectAssembly projectAssembly,
			_ISourceInformationProvider sourceInformationProvider)
		{
			this.diagnosticMessageSink = Guard.ArgumentNotNull(nameof(diagnosticMessageSink), diagnosticMessageSink);
			this.projectAssembly = Guard.ArgumentNotNull(nameof(projectAssembly), projectAssembly);
			this.sourceInformationProvider = Guard.ArgumentNotNull(nameof(sourceInformationProvider), sourceInformationProvider);

			Guard.NotNull("We do not yet support dynamic assemblies for Xunit3", projectAssembly.AssemblyFilename);

			runnerEngine = new TcpRunnerEngine("engine-id-tbd", OnMessage, diagnosticMessageSink);
			disposalTracker.Add(runnerEngine);

			var port = runnerEngine.Start();
			diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"v3 TCP Server running on tcp://localhost:{port}/ for '{projectAssembly.AssemblyFilename}'" });

			var startInfo = new ProcessStartInfo
			{
				Arguments = $"-tcp {port}",
				CreateNoWindow = true,
				FileName = projectAssembly.AssemblyFilename,
				WindowStyle = ProcessWindowStyle.Hidden,
			};

			var workingDirectory = Path.GetDirectoryName(projectAssembly.AssemblyFilename);
			if (!string.IsNullOrWhiteSpace(workingDirectory))
				startInfo.WorkingDirectory = workingDirectory;

			process = Guard.NotNull("Got a null process from Process.Start", Process.Start(startInfo));
		}

		/// <inheritdoc/>
		public bool CanUseAppDomains => false;

		/// <inheritdoc/>
		public string TargetFramework => projectAssembly.TargetFramework;

		/// <inheritdoc/>
		public string TestAssemblyUniqueID
		{
			get
			{
				WaitForEngineReady();
				return runnerEngine.TestAssemblyUniqueID;
			}
		}

		/// <inheritdoc/>
		public string TestFrameworkDisplayName
		{
			get
			{
				WaitForEngineReady();
				return runnerEngine.TestFrameworkDisplayName;
			}
		}

		/// <inheritdoc/>
		public async ValueTask DisposeAsync()
		{
			await disposalTracker.DisposeAsync();

			if (!process.WaitForExit(5000))
				diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Child process {process.Id} did not exit within 5 seconds; may need to be manually stopped" });
		}

		/// <inheritdoc/>
		public void Find(
			_IMessageSink discoveryMessageSink,
			_ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			WaitForEngineReady();

			var operationID = Guid.NewGuid().ToString("n");
			operations.TryAdd(operationID, discoveryMessageSink);
			runnerEngine.SendFind(operationID);
		}

		/// <inheritdoc/>
		public void Find(
			string typeName,
			_IMessageSink discoveryMessageSink,
			_ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			WaitForEngineReady();
			throw new NotImplementedException();
		}

		bool OnMessage(string operationID, _MessageSinkMessage message)
		{
			if (operations.TryGetValue(operationID, out var messageSink))
				return messageSink.OnMessage(message);

			return true;
		}

		/// <inheritdoc/>
		public void RunAll(
			_IMessageSink executionMessageSink,
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestFrameworkExecutionOptions executionOptions)
		{
			WaitForEngineReady();
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public void RunTests(
			IEnumerable<_ITestCase> testCases,
			_IMessageSink executionMessageSink,
			_ITestFrameworkExecutionOptions executionOptions)
		{
			WaitForEngineReady();
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public void RunTests(
			IEnumerable<string> serializedTestCases,
			_IMessageSink executionMessageSink,
			_ITestFrameworkExecutionOptions executionOptions)
		{
			WaitForEngineReady();
			throw new NotImplementedException();
		}

		bool WaitForEngineReady(int milliseconds = 30000)
		{
			if (runnerEngine.State.HasReachedConnectedState())
				return true;

			return Task.Run(async () =>
			{
				var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(milliseconds));

				while (true)
				{
					if (cancellationTokenSource.IsCancellationRequested)
						return false;

					if (runnerEngine.State.HasReachedConnectedState())
						return true;

					await Task.Delay(50, cancellationTokenSource.Token);
				}
			}).ConfigureAwait(false).GetAwaiter().GetResult();
		}
	}
}
