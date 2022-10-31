using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v3;

/// <summary>
/// The execution-side engine used to host an xUnit.net v3 test assembly that communicates via
/// TCP to the remote side, which is running <see cref="TcpRunnerEngine"/>. After connecting to
/// the TCP port, responds to commands from the runner engine, which translate/ to commands on
/// the <see cref="_ITestFrameworkDiscoverer"/> and <see cref="_ITestFrameworkExecutor"/>.
/// </summary>
public class TcpExecutionEngine : TcpEngine
{
	readonly ReflectionAssemblyInfo assemblyInfo;
	readonly BufferedTcpClient bufferedClient;
	readonly HashSet<string> cancellationRequested = new();
	readonly HashSet<string> executingOperations = new();
	readonly int port;
	readonly XunitProjectAssembly projectAssembly;
	readonly TaskCompletionSource<int> shutdownRequested = new();
	readonly Socket socket;
	readonly _ITestFramework testFramework;

	/// <summary>
	/// Initializes a new instance of the <see cref="TcpExecutionEngine"/> class.
	/// </summary>
	/// <param name="engineID">Engine ID (used for diagnostic messages).</param>
	/// <param name="project">The test project.</param>
	public TcpExecutionEngine(
		string engineID,
		XunitProject project) :
			base(engineID)
	{
		AddCommandHandler(TcpEngineMessages.Runner.Cancel, OnCancel);
		AddCommandHandler(TcpEngineMessages.Runner.Find, OnFind);
		AddCommandHandler(TcpEngineMessages.Runner.Info, OnInfo);
		AddCommandHandler(TcpEngineMessages.Runner.Quit, OnQuit);
		AddCommandHandler(TcpEngineMessages.Runner.Run, OnRun);

		Guard.ArgumentNotNull(project);
		Guard.ArgumentValid("Project must have exactly one test assembly", project.Assemblies.Count == 1, nameof(project));

		port = Guard.ArgumentNotNull(project.Configuration.TcpPort);

		projectAssembly = project.Assemblies.Single();

		Guard.NotNull("project.Assemblies[0].Assembly cannot be null", projectAssembly.Assembly);

		assemblyInfo = new ReflectionAssemblyInfo(projectAssembly.Assembly);
		testFramework = ExtensibilityPointFactory.GetTestFramework(assemblyInfo);
		DisposalTracker.Add(testFramework);

		socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
		bufferedClient = new BufferedTcpClient(string.Format(CultureInfo.CurrentCulture, "execution::{0}", engineID), socket, ProcessRequest, this);

		State = TcpEngineState.Initialized;

		DisposalTracker.AddAsyncAction(async () =>
		{
			SendInternalDiagnosticMessage("{0}: [INF] Disconnecting from tcp://localhost:{1}/", EngineDisplayName, port);

			try
			{
				await bufferedClient.DisposeAsync();
			}
			catch (Exception ex)
			{
				SendInternalDiagnosticMessage("{0}: [ERR] Error during buffered client disposal: {1}", EngineDisplayName, ex);
			}

			try
			{
				socket.Shutdown(SocketShutdown.Receive);
				socket.Shutdown(SocketShutdown.Send);
				socket.Close();
				socket.Dispose();
			}
			catch (Exception ex)
			{
				SendInternalDiagnosticMessage("{0}: [ERR] Error during connection socket shutdown: {1}", EngineDisplayName, ex);
			}

			SendInternalDiagnosticMessage("{0}: [INF] Disconnected from tcp://localhost:{1}/", EngineDisplayName, port);
		});
	}

	Task Execute(string operationID)
	{
		return Task.Run(async () =>
		{
			executingOperations.Add(operationID);

			SendInternalDiagnosticMessage("{0}: [INF] RUN started for operation ID '{1}'", EngineDisplayName, operationID);

			try
			{
				var discoveryOptions = _TestFrameworkOptions.ForDiscovery(projectAssembly.Configuration);
				var executionOptions = _TestFrameworkOptions.ForExecution(projectAssembly.Configuration);
				var frontController = new InProcessFrontController(testFramework, assemblyInfo, projectAssembly.ConfigFileName);
				var callbackSink = new EngineCallbackSink(this, operationID);
				IExecutionSink resultsSink = new DelegatingSummarySink(
					projectAssembly,
					discoveryOptions,
					executionOptions,
					AppDomainOption.NotAvailable,
					shadowCopy: false,
					callbackSink,
					() => !cancellationRequested.Contains(operationID)
				);
				var longRunningSeconds = projectAssembly.Configuration.LongRunningTestSecondsOrDefault;
				if (longRunningSeconds > 0)
					resultsSink = new DelegatingLongRunningTestDetectionSink(resultsSink, TimeSpan.FromSeconds(longRunningSeconds), this);
				if (projectAssembly.Configuration.FailSkipsOrDefault)
					resultsSink = new DelegatingFailSkipSink(resultsSink);

				using (resultsSink)
				{
					await frontController.FindAndRun(resultsSink, discoveryOptions, executionOptions, projectAssembly.Configuration.Filters.Filter);

					if (projectAssembly.Configuration.StopOnFailOrDefault && resultsSink.ExecutionSummary.Failed != 0)
					{
						SendInternalDiagnosticMessage("{0}: [WRN] Cancelling all operations because operation ID '{1}' ended in failure", EngineDisplayName, operationID);

						foreach (var executingOperation in executingOperations)
							cancellationRequested.Add(executingOperation);
					}
				}
			}
			catch (Exception ex)
			{
				var errorMessage = _ErrorMessage.FromException(ex);
				SendMessage(operationID, errorMessage);
			}

			executingOperations.Remove(operationID);
			cancellationRequested.Remove(operationID);

			SendInternalDiagnosticMessage("{0}: [INF] RUN finished for operation ID '{1}'", EngineDisplayName, operationID);
		});
	}

	Task Find(string operationID)
	{
		return Task.Run(async () =>
		{
			executingOperations.Add(operationID);

			SendInternalDiagnosticMessage("{0}: [INF] FIND started for operation ID '{1}'", EngineDisplayName, operationID);

			try
			{
				var discoveryOptions = _TestFrameworkOptions.ForDiscovery(projectAssembly.Configuration);
				var frontController = new InProcessFrontController(testFramework, assemblyInfo, projectAssembly.ConfigFileName);
				var callbackSink = new EngineCallbackSink(this, operationID);

				await frontController.Find(
					callbackSink,
					discoveryOptions,
					projectAssembly.Configuration.Filters.Filter,
					discoveryCallback: (testCase, passedFilter) =>
					{
						if (passedFilter)
						{
							var testCaseDiscovered = new _TestCaseDiscovered
							{
								AssemblyUniqueID = frontController.TestAssemblyUniqueID,
								Serialization = "" /*SerializationHelper.Serialize(testCase)*/,
								SkipReason = testCase.SkipReason,
								SourceFilePath = testCase.SourceFilePath,
								SourceLineNumber = testCase.SourceLineNumber,
								TestCaseDisplayName = testCase.TestCaseDisplayName,
								TestCaseUniqueID = testCase.UniqueID,
								TestClassName = testCase.TestClassName,
								TestClassNamespace = testCase.TestClassNamespace,
								TestClassNameWithNamespace = testCase.TestClassNameWithNamespace,
								TestClassUniqueID = testCase.TestMethod?.TestClass.UniqueID,
								TestCollectionUniqueID = testCase.TestCollection.UniqueID,
								TestMethodName = testCase.TestMethodName,
								TestMethodUniqueID = testCase.TestMethod?.UniqueID,
								Traits = testCase.Traits
							};

							SendMessage(operationID, testCaseDiscovered);
						}

						return new(!cancellationRequested.Contains(operationID));
					});
			}
			catch (Exception ex)
			{
				var errorMessage = _ErrorMessage.FromException(ex);
				SendMessage(operationID, errorMessage);
			}

			executingOperations.Remove(operationID);
			cancellationRequested.Remove(operationID);

			SendInternalDiagnosticMessage("{0}: [INF] FIND finished for operation ID '{1}'", EngineDisplayName, operationID);
		});
	}

	void OnCancel(ReadOnlyMemory<byte>? data)
	{
		lock (StateLock)
		{
			if (State != TcpEngineState.Connected)
			{
				SendInternalDiagnosticMessage("{0}: [ERR] Ignoring CANCEL message received outside of {1} state (current state is {2})", EngineDisplayName, TcpEngineState.Connecting, State);
				return;
			}
		}

		if (!data.HasValue || data.Value.Length == 0)
		{
			SendInternalDiagnosticMessage("{0}: [ERR] CANCEL data is missing the operation ID", EngineDisplayName);
			return;
		}

		var operationID = Encoding.UTF8.GetString(data.Value.ToArray());
		if (!executingOperations.Contains(operationID))
		{
			SendInternalDiagnosticMessage("{0}: [ERR] CANCEL requested for unknown operation ID '{1}'", EngineDisplayName, operationID);
			return;
		}

		SendInternalDiagnosticMessage("{0}: [INF] CANCEL request recorded for operation ID '{1}'", EngineDisplayName, operationID);
		cancellationRequested.Add(operationID);
	}

	void OnFind(ReadOnlyMemory<byte>? data)
	{
		lock (StateLock)
		{
			if (State != TcpEngineState.Connected)
			{
				SendInternalDiagnosticMessage("{0}: [ERR] Ignoring FIND message received outside of {1} state (current state is {2})", EngineDisplayName, TcpEngineState.Connected, State);
				return;
			}
		}

		if (!data.HasValue || data.Value.Length == 0)
		{
			SendInternalDiagnosticMessage("{0}: [ERR] FIND data is missing the operation ID", EngineDisplayName);
			return;
		}

		var operationID = Encoding.UTF8.GetString(data.Value.ToArray());
		if (executingOperations.Contains(operationID))
		{
			SendInternalDiagnosticMessage("{0}: [ERR] FIND requested for duplicate operation ID '{1}'", EngineDisplayName, operationID);
			return;
		}

		_ = Find(operationID);
	}

	void OnInfo(ReadOnlyMemory<byte>? _)
	{
		lock (StateLock)
		{
			if (State != TcpEngineState.Negotiating)
				SendInternalDiagnosticMessage("{0}: [ERR] INFO message received outside of {1} state (current state is {2})", EngineDisplayName, TcpEngineState.Negotiating, State);
			else
				State = TcpEngineState.Connected;
		}

		var testDiscoverer = testFramework.GetDiscoverer(assemblyInfo);
		var result = new TcpExecutionEngineInfo
		{
			TestAssemblyUniqueID = testDiscoverer.TestAssembly.UniqueID,
			TestFrameworkDisplayName = testDiscoverer.TestFrameworkDisplayName
		};

		bufferedClient.Send(TcpEngineMessages.Execution.Info);
		bufferedClient.Send(TcpEngineMessages.Separator);
		bufferedClient.Send(JsonSerializer.Serialize(result));
		bufferedClient.Send(TcpEngineMessages.EndOfMessage);
	}

	void OnQuit(ReadOnlyMemory<byte>? _) =>
		shutdownRequested.TrySetResult(0);

	void OnRun(ReadOnlyMemory<byte>? data)
	{
		lock (StateLock)
		{
			if (State != TcpEngineState.Connected)
			{
				SendInternalDiagnosticMessage("{0}: [ERR] Ignoring RUN message received outside of {1} state (current state is {2})", EngineDisplayName, TcpEngineState.Connected, State);
				return;
			}
		}

		if (!data.HasValue || data.Value.Length == 0)
		{
			SendInternalDiagnosticMessage("{0}: [ERR] RUN data is missing the operation ID", EngineDisplayName);
			return;
		}

		var operationID = Encoding.UTF8.GetString(data.Value.ToArray());
		if (executingOperations.Contains(operationID))
		{
			SendInternalDiagnosticMessage("{0}: [ERR] RUN requested for duplicate operation ID '{1}'", EngineDisplayName, operationID);
			return;
		}

		_ = Execute(operationID);
	}

	/// <inheritdoc/>
	protected override void SendDiagnosticMessage(
		string format,
		params object[] args)
	{
		var msg = new _DiagnosticMessage { Message = string.Format(CultureInfo.CurrentCulture, format, args) };

		SendMessage(BroadcastOperationID, msg);
		TestContext.Current?.DiagnosticMessageSink?.OnMessage(msg);
	}

	/// <inheritdoc/>
	protected override void SendInternalDiagnosticMessage(
		string format,
		params object[] args)
	{
		var msg = new _InternalDiagnosticMessage { Message = string.Format(CultureInfo.CurrentCulture, format, args) };

		SendMessage(BroadcastOperationID, msg);
		TestContext.Current?.InternalDiagnosticMessageSink?.OnMessage(msg);
	}

	/// <summary>
	/// Sends <see cref="TcpEngineMessages.Execution.Message"/>.
	/// </summary>
	/// <param name="operationID">The operation ID that this message is for.</param>
	/// <param name="message">The message to be sent.</param>
	/// <returns>Returns <c>true</c> if the operation should continue to run tests; <c>false</c> if it should cancel the run.</returns>
	public bool SendMessage(
		string operationID,
		_MessageSinkMessage message)
	{
		Guard.ArgumentNotNull(operationID);
		Guard.ArgumentNotNull(message);
		Guard.NotNull("Tried to call SendMessage when there is no remote engine available yet", bufferedClient);

		if (!bufferedClient.Disposed)
		{
			bufferedClient.Send(TcpEngineMessages.Execution.Message);
			bufferedClient.Send(TcpEngineMessages.Separator);
			bufferedClient.Send(operationID);
			bufferedClient.Send(TcpEngineMessages.Separator);
			bufferedClient.Send(message.ToJson());
			bufferedClient.Send(TcpEngineMessages.EndOfMessage);
		}

		return !cancellationRequested.Contains(operationID);
	}

	/// <summary>
	/// Starts the execution engine, connecting back to the runner engine on the TCP port
	/// provided to the constructor.
	/// </summary>
	/// <returns>The local port used for the conection.</returns>
	public async ValueTask<int> Start()
	{
		lock (StateLock)
		{
			if (State != TcpEngineState.Initialized)
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "{0}: Cannot call {1} in any state other than {2} (currently in state {3})", EngineDisplayName, nameof(Start), TcpEngineState.Initialized, State));

			State = TcpEngineState.Connecting;
		}

		SendInternalDiagnosticMessage("{0}: [INF] Connecting to tcp://localhost:{1}/", EngineDisplayName, port);

		await socket.ConnectAsync(IPAddress.Loopback, port);

		SendInternalDiagnosticMessage("{0}: [INF] Connected to tcp://localhost:{1}/", EngineDisplayName, port);

		lock (StateLock)
			State = TcpEngineState.Negotiating;

		bufferedClient.Start();

		return ((IPEndPoint?)socket.LocalEndPoint)?.Port ?? throw new InvalidOperationException("Could not retrieve port from socket local endpoint");
	}

	/// <summary>
	/// Waits for the QUIT signal from the runner engine.
	/// </summary>
	// TODO: CancellationToken? Timespan for timeout?
	public Task WaitForQuit() =>
		shutdownRequested.Task;

	sealed class EngineCallbackSink : _IMessageSink
	{
		readonly TcpExecutionEngine engine;
		readonly string operationID;

		public EngineCallbackSink(
			TcpExecutionEngine engine,
			string operationID)
		{
			this.engine = engine;
			this.operationID = operationID;
		}

		public bool OnMessage(_MessageSinkMessage message)
		{
			engine.SendMessage(operationID, message);
			return !engine.cancellationRequested.Contains(operationID);
		}
	}
}
