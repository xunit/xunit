using System;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
/// <summary/>
public class SynchronousMessageBus(
	_IMessageSink messageSink,
	bool stopOnFail = false) :
		IMessageBus
{
	volatile bool continueRunning = true;
	readonly _IMessageSink messageSink = Guard.ArgumentNotNull(messageSink);
	readonly bool stopOnFail = stopOnFail;

	/// <summary/>
	public void Dispose() =>
		GC.SuppressFinalize(this);

	/// <summary/>
	public bool QueueMessage(_MessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		if (stopOnFail && message is _TestFailed)
			continueRunning = false;

		continueRunning = messageSink.OnMessage(message) && continueRunning;
		return continueRunning;
	}
}
