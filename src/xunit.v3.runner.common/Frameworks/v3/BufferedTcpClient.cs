using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.v3;

/// <summary>
/// Provides a line-oriented read/write wrapper over top of a TCP socket.
/// </summary>
public class BufferedTcpClient : IAsyncDisposable
{
	bool disposed;
	readonly DisposalTracker disposalTracker = new();
	Exception? fault;
	readonly TaskCompletionSource<int> finishedSource = new();
	readonly Action<ReadOnlyMemory<byte>> receiveHandler;
	readonly Socket socket;
	readonly List<Task> tasks = new();
	readonly AutoResetEvent writeEvent = new(initialState: false);
	readonly ConcurrentQueue<byte[]> writeQueue = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="BufferedTcpClient"/> class.
	/// </summary>
	/// <param name="socket">The TCP socket that is read from/written to.</param>
	/// <param name="receiveHandler">The handler that is called for each received line of text.</param>
	public BufferedTcpClient(
		Socket socket,
		Action<ReadOnlyMemory<byte>> receiveHandler)
	{
		Guard.ArgumentNotNull(socket);

		this.socket = socket;
		this.receiveHandler = Guard.ArgumentNotNull(receiveHandler);
	}

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		if (disposed)
			return;

		if (fault is not null)
		{
			var tcs = new TaskCompletionSource<int>();
			tcs.SetException(fault);
			await tcs.Task;
		}

		disposed = true;

		GC.SuppressFinalize(this);

		finishedSource.TrySetResult(0);
		writeEvent.Set();

		await Task.WhenAll(tasks);
		await disposalTracker.DisposeAsync();

		writeEvent.Dispose();
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
		}
		catch (Exception ex)
		{
			fault = ex;
		}

		await reader.CompleteAsync();
	}

	async Task StartSocketPipeWriter()
	{
		var stream = new NetworkStream(socket);
		disposalTracker.Add(stream);

		var writer = PipeWriter.Create(stream);

		try
		{
			while (true)
			{
				writeEvent.WaitOne();

				while (writeQueue.TryDequeue(out var bytes))
					await writer.WriteAsync(bytes);

				if (finishedSource.Task.IsCompleted)
					break;
			}
		}
		catch (Exception ex)
		{
			fault = ex;
		}

		await writer.CompleteAsync();
		await writer.FlushAsync();
	}

	static bool TryFindCommand(
		ref ReadOnlySequence<byte> buffer,
		out byte[]? line)
	{
		var position = buffer.PositionOf(TcpEngineMessages.EndOfMessage[0]);

		if (position is null)
		{
			line = default;
			return false;
		}

		line = buffer.Slice(0, position.Value).ToArray();
		buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
		return true;
	}
}
