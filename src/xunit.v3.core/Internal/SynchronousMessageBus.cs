using System;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
/// <summary/>
public class SynchronousMessageBus(
	IMessageSink messageSink,
	bool stopOnFail = false) :
		IMessageBus
{
	volatile bool continueRunning = true;
	readonly IMessageSink messageSink = Guard.ArgumentNotNull(messageSink);
	readonly bool stopOnFail = stopOnFail;

	/// <summary/>
	public void Dispose() =>
		GC.SuppressFinalize(this);

	/// <summary/>
	public bool QueueMessage(IMessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		if (stopOnFail && message is ITestFailed)
			continueRunning = false;

		continueRunning = messageSink.OnMessage(message) && continueRunning;
		return continueRunning;
	}
}
