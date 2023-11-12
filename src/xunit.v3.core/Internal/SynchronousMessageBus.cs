using System;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public class SynchronousMessageBus : IMessageBus
{
	volatile bool continueRunning = true;
	readonly _IMessageSink messageSink;
	readonly bool stopOnFail;

	/// <summary/>
	public SynchronousMessageBus(
		_IMessageSink messageSink,
		bool stopOnFail = false)
	{
		this.messageSink = Guard.ArgumentNotNull(messageSink);
		this.stopOnFail = stopOnFail;
	}

	/// <summary/>
	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	/// <summary/>
	public bool QueueMessage(_MessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		if (stopOnFail && message is _TestFailed)
			continueRunning = false;

		return messageSink.OnMessage(message) && continueRunning;
	}
}
