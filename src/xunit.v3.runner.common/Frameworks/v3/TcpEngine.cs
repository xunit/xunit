using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v3
{
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
			_IMessageSink diagnosticMessageSink)
		{
			EngineID = Guard.ArgumentNotNullOrEmpty(nameof(engineID), engineID);
			DiagnosticMessageSink = Guard.ArgumentNotNull(nameof(diagnosticMessageSink), diagnosticMessageSink);
		}

		/// <summary>
		/// Gets the diagnostic message sink to send diagnostic messages to.
		/// </summary>
		protected _IMessageSink DiagnosticMessageSink { get; }

		/// <summary>
		/// Gets the disposal tracker that's automatically cleaned up during <see cref="DisposeAsync"/>.
		/// </summary>
		protected DisposalTracker DisposalTracker { get; } = new();

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
				DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"{GetType().Name}({EngineID}): Engine state transition from {state} to {value}" });
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
			lock (StateLock)
			{
				if (State == TcpEngineState.Disconnecting || State == TcpEngineState.Disconnected)
					throw new ObjectDisposedException($"{GetType().Name}({EngineID})");

				State = TcpEngineState.Disconnecting;
			}

			try
			{
				await DisposalTracker.DisposeAsync();
			}
			catch (Exception ex)
			{
				DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"{GetType().Name}({EngineID}): Error during disposal: {ex}" });
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
						DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"{GetType().Name}({EngineID}): Error during message processing '{Encoding.UTF8.GetString(request.ToArray())}': {ex}" });
					}

					return;
				}

			DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"{GetType().Name}({EngineID}): Received unknown command '{Encoding.UTF8.GetString(request.ToArray())}'" });
		}
	}
}
