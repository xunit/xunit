using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v3
{
	/// <summary>
	/// Provides a line-oriented read/write wrapper over top of a TCP socket. Intended to be used
	/// on both sides of the v3 TCP-based reporter system (<see cref="T:Xunit.Runner.v3.TcpExecutionEngine"/> and
	/// <see cref="TcpRunnerEngine"/>).
	/// </summary>
	public class BufferedTcpClient : IAsyncDisposable
	{
		readonly string clientID;
		readonly _IMessageSink diagnosticMessageSink;
		bool disposed = false;
		readonly DisposalTracker disposalTracker = new();
		readonly TaskCompletionSource<int> finishedSource = new();
		readonly Action<ReadOnlyMemory<byte>> receiveHandler;
		readonly Socket socket;
		readonly List<Task> tasks = new();
		readonly AutoResetEvent writeEvent = new(initialState: false);
		readonly ConcurrentQueue<byte[]> writeQueue = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="BufferedTcpClient"/> class.
		/// </summary>
		/// <param name="clientID">The client ID (used in diagnostic messages).</param>
		/// <param name="socket">The TCP socket that is read from/written to.</param>
		/// <param name="receiveHandler">The handler that is called for each received line of text.</param>
		/// <param name="diagnosticMessageSink">The message sink to send diagnostic messages to.</param>
		public BufferedTcpClient(
			string clientID,
			Socket socket,
			Action<ReadOnlyMemory<byte>> receiveHandler,
			_IMessageSink diagnosticMessageSink)
		{
			this.clientID = Guard.ArgumentNotNull(nameof(clientID), clientID);
			this.socket = Guard.ArgumentNotNull(nameof(socket), socket);
			this.receiveHandler = Guard.ArgumentNotNull(nameof(receiveHandler), receiveHandler);
			this.diagnosticMessageSink = Guard.ArgumentNotNull(nameof(diagnosticMessageSink), diagnosticMessageSink);
		}

		/// <inheritdoc/>
		public async ValueTask DisposeAsync()
		{
			if (disposed)
				throw new ObjectDisposedException(typeof(BufferedTcpClient).FullName);

			disposed = true;

			finishedSource.TrySetResult(0);
			writeEvent.Set();

			await Task.WhenAll(tasks);
			await disposalTracker.DisposeAsync();
		}

		/// <summary>
		/// Sends bytes to the other side of the connection.
		/// </summary>
		/// <param name="bytes">The bytes to send to the other side of the connection.</param>
		public void Send(byte[] bytes)
		{
			if (disposed)
				throw new ObjectDisposedException(typeof(BufferedTcpClient).FullName);

			writeQueue.Enqueue(bytes);
			writeEvent.Set();
		}

		/// <summary>
		/// Encodes a string value as UTF8 bytes and sends those bytes to the other side of the connection.
		/// </summary>
		/// <param name="value">The value to send to the other side of the connection.</param>
		public void Send(string value) =>
			Send(Encoding.UTF8.GetBytes(value));

		/// <summary>
		/// Starts the read/write background workers.
		/// </summary>
		public void Start()
		{
			if (tasks.Count != 0)
				throw new InvalidOperationException("Cannot call Start more the one time");

			tasks.Add(Task.Run(StartSocketPipeReader));
			tasks.Add(Task.Run(StartSocketPipeWriter));
		}

		async Task StartSocketPipeReader()
		{
			var stream = new NetworkStream(socket);
			disposalTracker.Add(stream);

			var reader = PipeReader.Create(stream);

			try
			{
				while (true)
				{
					var readTask = reader.ReadAsync().AsTask();
					var completedTask = await Task.WhenAny(readTask, finishedSource.Task);
					if (completedTask == finishedSource.Task)
					{
						reader.CancelPendingRead();
						break;
					}

					var result = await readTask;
					var buffer = result.Buffer;

					while (TryFindCommand(ref buffer, out var line))
						try
						{
							receiveHandler(line);
						}
						catch { }  // Ignore the handler throwing; that's their problem, not ours.

					reader.AdvanceTo(buffer.Start, buffer.End);

					if (result.IsCompleted)
						break;
				}

				await reader.CompleteAsync();
			}
			catch (Exception ex)
			{
				diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"BufferTcpClient({clientID}): abnormal termination of pipe reader: {ex}" });
			}
		}

		async Task StartSocketPipeWriter()
		{
			var stream = new NetworkStream(socket);
			disposalTracker.Add(stream);

			var pipeWriter = PipeWriter.Create(stream);

			try
			{
				while (true)
				{
					writeEvent.WaitOne();

					while (writeQueue.TryDequeue(out var bytes))
						await pipeWriter.WriteAsync(bytes);

					await pipeWriter.FlushAsync();

					if (finishedSource.Task.IsCompleted)
						break;
				}

				await pipeWriter.CompleteAsync();
			}
			catch (Exception ex)
			{
				diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"BufferTcpClient({clientID}): abnormal termination of pipe writer: {ex}" });
			}
		}

		bool TryFindCommand(
			ref ReadOnlySequence<byte> buffer,
			out byte[]? line)
		{
			var position = buffer.PositionOf(TcpEngineMessages.EndOfMessage[0]);

			if (position == null)
			{
				line = default;
				return false;
			}

			line = buffer.Slice(0, position.Value).ToArray();
			buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
			return true;
		}
	}
}
