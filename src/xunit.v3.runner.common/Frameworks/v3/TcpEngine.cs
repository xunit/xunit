using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v3;

/// <summary>
/// A base class used for TCP engines (specifically, <see cref="TcpRunnerEngine"/> and
/// <see cref="T:Xunit.Runner.v3.TcpExecutionEngine"/>).
/// </summary>
public class TcpEngine : IAsyncDisposable
{
	readonly List<(byte[] command, Action<ReadOnlyMemory<byte>?> handler)> commandHandlers = new();
	TcpEngineState state = TcpEngineState.Unknown;

	/// <summary>
	/// Initializes a new instance of the <see cref="TcpEngine"/> class.
	/// </summary>
	/// <param name="engineID">The engine ID (used for diagnostic messages).</param>
	/// <param name="diagnosticMessageSink">The diagnostic message sink to send diagnostic messages to.</param>
	public TcpEngine(
		string engineID,
		_IMessageSink? diagnosticMessageSink)
	{
		EngineID = Guard.ArgumentNotNullOrEmpty(engineID);
		EngineDisplayName = string.Format(CultureInfo.CurrentCulture, "{0}({1})", GetType().Name, engineID);
		DiagnosticMessageSink = diagnosticMessageSink;
	}

	/// <summary>
	/// Gets the diagnostic message sink to send diagnostic messages to.
	/// </summary>
	protected _IMessageSink? DiagnosticMessageSink { get; }

	/// <summary>
	/// Gets the disposal tracker that's automatically cleaned up during <see cref="DisposeAsync"/>.
	/// </summary>
	protected DisposalTracker DisposalTracker { get; } = new();

	/// <summary>
	/// Gets the display name for the current engine, for formatting diagnostic messages.
	/// </summary>
	protected string EngineDisplayName { get; }

	/// <summary>
	/// Gets the engine ID.
	/// </summary>
	protected string EngineID { get; }

	/// <summary>
	/// Gets the current state of the engine.
	/// </summary>
	public TcpEngineState State
	{
		get => state;
		protected set
		{
			// TODO: Should we offer an event for state changes?
			DiagnosticMessageSink?.OnMessage(new _DiagnosticMessage("{0}: Engine state transition from {1} to {2}", EngineDisplayName, state, value));

			state = value;
		}
	}

	/// <summary>
	/// An object which can be used for locks which test and change state.
	/// </summary>
	protected object StateLock { get; } = new();

	/// <summary>
	/// Adds a command handler to the engine.
	/// </summary>
	/// <param name="command">The command (in byte array form) to be handled</param>
	/// <param name="handler">The handler to be called when the command is issued</param>
	protected void AddCommandHandler(byte[] command, Action<ReadOnlyMemory<byte>?> handler) =>
		commandHandlers.Add((command, handler));

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		lock (StateLock)
		{
			if (State == TcpEngineState.Disconnecting || State == TcpEngineState.Disconnected)
				throw new ObjectDisposedException(EngineDisplayName);

			State = TcpEngineState.Disconnecting;
		}

		try
		{
			await DisposalTracker.DisposeAsync();
		}
		catch (Exception ex)
		{
			DiagnosticMessageSink?.OnMessage(new _DiagnosticMessage("{0}: Error during disposal: {1}", EngineDisplayName, ex));
		}

		lock (StateLock)
			State = TcpEngineState.Disconnected;
	}

	/// <summary>
	/// Processes a request provided by the <see cref="BufferedTcpClient"/>. Dispatches to
	/// the appropriate command handler, as registered with <see cref="AddCommandHandler"/>.
	/// </summary>
	/// <param name="request">The received request.</param>
	protected void ProcessRequest(ReadOnlyMemory<byte> request)
	{
		var (command, data) = TcpEngineMessages.SplitOnSeparator(request);

		foreach (var commandHandler in commandHandlers)
			if (command.Span.SequenceEqual(commandHandler.command))
			{
				try
				{
					commandHandler.handler(data);
				}
				catch (Exception ex)
				{
					DiagnosticMessageSink?.OnMessage(new _DiagnosticMessage("{0}: Error during message processing '{1}': {2}", EngineDisplayName, Encoding.UTF8.GetString(request.ToArray()), ex));
				}

				return;
			}

		DiagnosticMessageSink?.OnMessage(new _DiagnosticMessage("{0}: Received unknown command '{1}'", EngineDisplayName, Encoding.UTF8.GetString(request.ToArray())));
	}
}
