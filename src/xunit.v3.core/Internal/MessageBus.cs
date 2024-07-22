using System;
using System.Collections.Concurrent;
using System.Threading;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public class MessageBus : IMessageBus
{
	volatile bool continueRunning = true;
	bool disposed;
	readonly IMessageSink messageSink;
	readonly ConcurrentQueue<IMessageSinkMessage> reporterQueue = new();
	readonly Thread reporterThread;
	readonly AutoResetEvent reporterWorkEvent = new(initialState: false);
	volatile bool shutdownRequested;
	readonly bool stopOnFail;

	/// <summary/>
	public MessageBus(
		IMessageSink messageSink,
		bool stopOnFail = false)
	{
		this.messageSink = messageSink;
		this.stopOnFail = stopOnFail;

		reporterThread = new Thread(ReporterWorker);
		reporterThread.Start();
	}

	void DispatchMessages()
	{
		while (reporterQueue.TryDequeue(out var message))
			try
			{
				continueRunning = messageSink.OnMessage(message) && continueRunning;
			}
			catch (Exception ex)
			{
				try
				{
					var errorMessage = ErrorMessage.FromException(ex);
					if (!messageSink.OnMessage(errorMessage))
						continueRunning = false;
				}
				catch { }
			}
	}

	/// <summary/>
	public void Dispose()
	{
		if (disposed)
			return;

		disposed = true;

		GC.SuppressFinalize(this);

		shutdownRequested = true;

		reporterWorkEvent.Set();
		reporterThread.Join();
		reporterWorkEvent.Dispose();
	}

	/// <summary/>
	public bool QueueMessage(IMessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		if (shutdownRequested)
			throw new ObjectDisposedException("MessageBus");

		if (stopOnFail && message is ITestFailed)
			continueRunning = false;

		reporterQueue.Enqueue(message);
		reporterWorkEvent.Set();
		return continueRunning;
	}

	void ReporterWorker()
	{
		while (!shutdownRequested)
		{
			reporterWorkEvent.WaitOne();
			DispatchMessages();
		}

		// One final dispatch pass
		DispatchMessages();
	}
}
