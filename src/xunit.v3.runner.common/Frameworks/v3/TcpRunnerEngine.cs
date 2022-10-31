using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v3;

/// <summary>
/// The runner-side engine used to host an xUnit.net v3 test assembly. Opens a port
/// for message communication, and translates the communication channel back into v3
/// message objects which are passed to the provided <see cref="_IMessageSink"/>.
/// Sends commands to the remote side, which is running <see cref="T:Xunit.Runner.v3.TcpExecutionEngine"/>.
/// </summary>
public class TcpRunnerEngine : TcpEngine, IAsyncDisposable
{
	BufferedTcpClient? bufferedClient;
	int cancelRequested;
	readonly _IMessageSink diagnosticMessageSink;
	readonly Func<string, _MessageSinkMessage, bool> messageDispatcher;
	bool quitSent;

	/// <summary>
	/// Initializes a new instance of the <see cref="TcpRunnerEngine"/> class.
	/// </summary>
	/// <param name="engineID">Engine ID (used for diagnostic messages).</param>
	/// <param name="messageDispatcher">The message dispatcher to pass remote messages to.</param>
	/// <param name="diagnosticMessageSink">The message sink to send diagnostic messages to.</param>
	public TcpRunnerEngine(
		string engineID,
		Func<string, _MessageSinkMessage, bool> messageDispatcher,
		_IMessageSink diagnosticMessageSink) :
			base(engineID)
	{
		State = TcpEngineState.Initialized;

		AddCommandHandler(TcpEngineMessages.Execution.Info, OnInfo);
		AddCommandHandler(TcpEngineMessages.Execution.Message, OnMessage);

		this.messageDispatcher = Guard.ArgumentNotNull(messageDispatcher);
		this.diagnosticMessageSink = Guard.ArgumentNotNull(diagnosticMessageSink);
	}

	/// <summary>
	/// Gets the <see cref="TcpExecutionEngineInfo"/> received during protocol negotiation. Will
	/// be <c>null</c> unless <see cref="TcpEngine.State"/> is <see cref="TcpEngineState.Connected"/>.
	/// </summary>
	public TcpExecutionEngineInfo? ExecutionEngineInfo { get; private set; }

	/// <summary>
	/// Gets the unique ID for the test assembly. If called before we have reached a <see cref="TcpEngine.State"/>
	/// of <see cref="TcpEngineState.Connected"/>, will throw <see cref="InvalidOperationException"/>.
	/// </summary>
	public string TestAssemblyUniqueID
	{
		get
		{
			if (ExecutionEngineInfo is null)
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "{0}: Cannot call {1} before it has reached the {2} state (currently in state {3})", EngineDisplayName, nameof(TestAssemblyUniqueID), TcpEngineState.Connected, State));

			return ExecutionEngineInfo.TestAssemblyUniqueID;
		}
	}

	/// <summary>
	/// Gets the test framework display name for the test assembly. If called before we have reached a <see cref="TcpEngine.State"/>
	/// of <see cref="TcpEngineState.Connected"/>, will throw <see cref="InvalidOperationException"/>.
	/// </summary>
	public string TestFrameworkDisplayName
	{
		get
		{
			if (ExecutionEngineInfo is null)
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "{0}: Cannot call {1} before it has reached the {2} state (currently in state {3})", EngineDisplayName, nameof(TestFrameworkDisplayName), TcpEngineState.Connected, State));

			return ExecutionEngineInfo.TestFrameworkDisplayName;
		}
	}

	void OnInfo(ReadOnlyMemory<byte>? data)
	{
		if (!data.HasValue)
		{
			SendInternalDiagnosticMessage("{0}: [ERR] INFO data is missing the JSON", EngineDisplayName);
			return;
		}

		ExecutionEngineInfo = JsonSerializer.Deserialize<TcpExecutionEngineInfo>(data.Value.Span);

		lock (StateLock)
		{
			if (State != TcpEngineState.Negotiating)
				SendInternalDiagnosticMessage("{0}: [ERR] INFO message received before we reached {1} state (current state is {2})", EngineDisplayName, TcpEngineState.Negotiating, State);
			else
				State = TcpEngineState.Connected;
		}
	}

	void OnMessage(ReadOnlyMemory<byte>? data)
	{
		if (!data.HasValue)
		{
			SendInternalDiagnosticMessage("{0}: [ERR] MSG data is missing the operation ID and JSON", EngineDisplayName);
			return;
		}

		var (requestID, json) = TcpEngineMessages.SplitOnSeparator(data.Value);

		if (!json.HasValue)
		{
			SendInternalDiagnosticMessage("{0}: [ERR] MSG data is missing the JSON", EngineDisplayName);
			return;
		}

		var deserializedMessage = _MessageSinkMessage.ParseJson(json.Value);
		var @continue = messageDispatcher(Encoding.UTF8.GetString(requestID.ToArray()), deserializedMessage);

		if (!@continue && Interlocked.Exchange(ref cancelRequested, 1) == 0)
		{
			bufferedClient?.Send(TcpEngineMessages.Runner.Cancel);
			bufferedClient?.Send(TcpEngineMessages.EndOfMessage);
		}
	}

	/// <summary>
	/// Sends <see cref="TcpEngineMessages.Runner.Cancel"/>.
	/// </summary>
	/// <param name="operationID">The operation ID to cancel.</param>
	public void SendCancel(string operationID)
	{
		Guard.ArgumentNotNull(operationID);

		if (bufferedClient is null)
		{
			SendInternalDiagnosticMessage("{0}: [ERR] {1} called when there is no connected execution engine", EngineDisplayName, nameof(SendCancel));
			return;
		}

		bufferedClient.Send(TcpEngineMessages.Runner.Cancel);
		bufferedClient.Send(TcpEngineMessages.Separator);
		bufferedClient.Send(operationID);
		bufferedClient.Send(TcpEngineMessages.EndOfMessage);
	}

	/// <inheritdoc/>
	protected override void SendDiagnosticMessage(
		string format,
		params object?[] args) =>
			diagnosticMessageSink?.OnMessage(new _DiagnosticMessage(format, args));

	/// <summary>
	/// Sends <see cref="TcpEngineMessages.Runner.Find"/>.
	/// </summary>
	/// <param name="operationID">The operation ID for the find operation.</param>
	public void SendFind(string operationID)
	{
		Guard.ArgumentNotNull(operationID);

		if (bufferedClient is null)
		{
			SendInternalDiagnosticMessage("{0}: {1} called when there is no connected execution engine", EngineDisplayName, nameof(SendFind));
			return;
		}

		bufferedClient.Send(TcpEngineMessages.Runner.Find);
		bufferedClient.Send(TcpEngineMessages.Separator);
		bufferedClient.Send(operationID);
		bufferedClient.Send(TcpEngineMessages.EndOfMessage);

		SendInternalDiagnosticMessage("{0}: [INF] Request sent: FIND {1}", EngineDisplayName, operationID);
	}

	/// <inheritdoc/>
	protected override void SendInternalDiagnosticMessage(
		string format,
		params object?[] args) =>
			diagnosticMessageSink?.OnMessage(new _InternalDiagnosticMessage { Message = string.Format(CultureInfo.CurrentCulture, format, args) });

	/// <summary>
	/// Sends <see cref="TcpEngineMessages.Runner.Quit"/>.
	/// </summary>
	public void SendQuit()
	{
		if (bufferedClient is null)
		{
			SendInternalDiagnosticMessage("{0}: [ERR] {1} called when there is no connected execution engine", EngineDisplayName, nameof(SendQuit));
			return;
		}

		quitSent = true;

		bufferedClient.Send(TcpEngineMessages.Runner.Quit);
		bufferedClient.Send(TcpEngineMessages.EndOfMessage);

		SendInternalDiagnosticMessage("{0}: [INF] Request sent: QUIT", EngineDisplayName);
	}

	/// <summary>
	/// Sends <see cref="TcpEngineMessages.Runner.Run"/>.
	/// </summary>
	/// <param name="operationID"></param>
	public void SendRun(string operationID)
	{
		Guard.ArgumentNotNull(operationID);

		if (bufferedClient is null)
		{
			SendInternalDiagnosticMessage("{0}: [ERR] {1} called when there is no connected execution engine", EngineDisplayName, nameof(SendRun));
			return;
		}

		bufferedClient.Send(TcpEngineMessages.Runner.Run);
		bufferedClient.Send(TcpEngineMessages.Separator);
		bufferedClient.Send(operationID);
		bufferedClient.Send(TcpEngineMessages.EndOfMessage);
	}

	/// <summary>
	/// Start the TCP server. Stop the server by disposing it.
	/// </summary>
	/// <returns>Returns the TCP port that the server is listening on.</returns>
	public int Start()
	{
		int listenPort;
		Socket listenSocket;

		lock (StateLock)
		{
			if (State != TcpEngineState.Initialized)
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "{0}: Cannot call {1} in any state other than {2} (currently in state {3})", EngineDisplayName, nameof(Start), TcpEngineState.Initialized, State));

#pragma warning disable CA2000 // This object is disposed via the disposal tracker
			listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
#pragma warning restore CA2000
			DisposalTracker.AddAction(() =>
			{
				try
				{
					listenSocket.Close();
					listenSocket.Dispose();
				}
				catch (Exception ex)
				{
					SendInternalDiagnosticMessage("{0}: [ERR] Error during listen socket closure: {1}", EngineDisplayName, ex);
				}
			});

			listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
			listenSocket.Listen(1);

			listenPort = ((IPEndPoint)listenSocket.LocalEndPoint!).Port;

			State = TcpEngineState.Listening;
		}

		Task.Run(async () =>
		{
			var socket = await listenSocket.AcceptAsync();
			listenSocket.Close();

			var remotePort = ((IPEndPoint?)socket.RemoteEndPoint)?.Port.ToString(CultureInfo.InvariantCulture) ?? "<unknown_port>";

			SendInternalDiagnosticMessage("{0}: [INF] Connection accepted from tcp://localhost:{1}/", EngineDisplayName, remotePort);

			DisposalTracker.AddAction(() =>
			{
				SendInternalDiagnosticMessage("{0}: [INF] Disconnecting from tcp://localhost:{1}/", EngineDisplayName, remotePort);

				try
				{
					socket.Shutdown(SocketShutdown.Receive);
					socket.Shutdown(SocketShutdown.Send);
					socket.Close();
					socket.Dispose();
				}
				catch (Exception ex)
				{
					SendInternalDiagnosticMessage("{0}: [ERR] Error during connection socket closure: {1}", EngineDisplayName, ex);
				}

				SendInternalDiagnosticMessage("{0}: [INF] Disconnected from tcp://localhost:{1}/", EngineDisplayName, remotePort);
			});

			bufferedClient = new(string.Format(CultureInfo.InvariantCulture, "runner::{0}", EngineID), socket, ProcessRequest, this)
			{
				OnAbnormalTermination = ex => messageDispatcher(BroadcastOperationID, _ErrorMessage.FromException(ex))
			};
			bufferedClient.Start();

			DisposalTracker.AddAsyncAction(async () =>
			{
				try
				{
					if (!quitSent)
						SendQuit();
				}
				catch (Exception ex)
				{
					SendInternalDiagnosticMessage("{0}: [ERR] Error sending QUIT message to execution engine: {1}", EngineDisplayName, ex);
				}

				try
				{
					await bufferedClient.DisposeAsync();
				}
				catch (Exception ex)
				{
					SendInternalDiagnosticMessage("{0}: [ERR] Error during buffered client disposal: {1}", EngineDisplayName, ex);
				}
			});

			// Send INFO message to start protocol negotiation
			lock (StateLock)
				State = TcpEngineState.Negotiating;

			var engineInfo = new TcpRunnerEngineInfo();

			bufferedClient.Send(TcpEngineMessages.Runner.Info);
			bufferedClient.Send(TcpEngineMessages.Separator);
			bufferedClient.Send(JsonSerializer.Serialize(engineInfo));
			bufferedClient.Send(TcpEngineMessages.EndOfMessage);
		});

		SendInternalDiagnosticMessage("{0}: [INF] Listening on tcp://localhost:{1}/", EngineDisplayName, listenPort);

		return listenPort;
	}
}
