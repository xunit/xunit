using System;
using System.Collections.Generic;
using System.Threading;
using NSubstitute;
using Xunit.Sdk;

public class SpyMessageSink : _IMessageSink
{
	readonly Func<_MessageSinkMessage, bool> callback;

	protected SpyMessageSink(Func<_MessageSinkMessage, bool>? callback = null) =>
		this.callback = callback ?? ((msg) => true);

	public List<_MessageSinkMessage> Messages { get; } = new();

	public static SpyMessageSink Capture(Func<_MessageSinkMessage, bool>? callback = null) =>
		new(callback);

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
		return callback?.Invoke(message) ?? true;
	}
}

public class SpyMessageSink<TFinalMessage> : SpyMessageSink, IDisposable
{
	bool disposed;

	SpyMessageSink(Func<_MessageSinkMessage, bool>? callback) :
		base(callback)
	{ }

	public ManualResetEvent Finished = new(initialState: false);

	public static SpyMessageSink<TFinalMessage> Create(Func<_MessageSinkMessage, bool>? callback = null) =>
		new(callback);

	public void Dispose()
	{
		if (disposed)
			return;

		disposed = true;

		Finished.Dispose();
	}

	public override bool OnMessage(_MessageSinkMessage message)
	{
		var result = base.OnMessage(message);

		if (message is TFinalMessage)
			Finished.Set();

		return result;
	}
}
