using System;
using System.Collections.Generic;
using System.Threading;
using NSubstitute;
using Xunit.v3;

public class SpyMessageSink : _IMessageSink
{
	public readonly List<_MessageSinkMessage> Messages = new();

	public static SpyMessageSink Capture() =>
		new();

	public static _IMessageSink Create(
		bool returnResult = true,
		List<_MessageSinkMessage>? messages = null) =>
			Create(_ => returnResult, messages);

	public static _IMessageSink Create(
		Func<_MessageSinkMessage, bool> lambda,
		List<_MessageSinkMessage>? messages = null)
	{
		var result = Substitute.For<_IMessageSink>();

		result
			.OnMessage(null!)
			.ReturnsForAnyArgs(callInfo =>
			{
				var message = callInfo.Arg<_MessageSinkMessage>();

				if (messages is not null)
					messages.Add(message);

				return lambda(message);
			});

		return result;
	}

	public virtual bool OnMessage(_MessageSinkMessage message)
	{
		Messages.Add(message);
		return true;
	}
}

public class SpyMessageSink<TFinalMessage> : SpyMessageSink, IDisposable
{
	readonly Func<_MessageSinkMessage, bool> cancellationThunk;
	bool disposed;

	SpyMessageSink(Func<_MessageSinkMessage, bool>? cancellationThunk)
	{
		this.cancellationThunk = cancellationThunk ?? (msg => true);
	}

	public ManualResetEvent Finished = new(initialState: false);

	public static SpyMessageSink<TFinalMessage> Create(Func<_MessageSinkMessage, bool>? cancellationThunk = null) =>
		new(cancellationThunk);

	public void Dispose()
	{
		if (disposed)
			return;

		disposed = true;

		Finished.Dispose();
	}

	public override bool OnMessage(_MessageSinkMessage message)
	{
		base.OnMessage(message);

		if (message is TFinalMessage)
			Finished.Set();

		return cancellationThunk(message);
	}
}
