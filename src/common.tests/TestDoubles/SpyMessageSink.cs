using System;
using System.Collections.Generic;
using System.Threading;
using NSubstitute;
using Xunit.Sdk;

public class SpyMessageSink : IMessageSink
{
	readonly Func<MessageSinkMessage, bool> callback;

	protected SpyMessageSink(Func<MessageSinkMessage, bool>? callback = null) =>
		this.callback = callback ?? ((msg) => true);

	public List<MessageSinkMessage> Messages { get; } = new();

	public static SpyMessageSink Capture(Func<MessageSinkMessage, bool>? callback = null) =>
		new(callback);

	public static IMessageSink Create(
		bool returnResult = true,
		List<MessageSinkMessage>? messages = null) =>
			Create(_ => returnResult, messages);

	public static IMessageSink Create(
		Func<MessageSinkMessage, bool> lambda,
		List<MessageSinkMessage>? messages = null)
	{
		var result = Substitute.For<IMessageSink>();

		result
			.OnMessage(null!)
			.ReturnsForAnyArgs(callInfo =>
			{
				var message = callInfo.Arg<MessageSinkMessage>();

				if (messages is not null)
					messages.Add(message);

				return lambda(message);
			});

		return result;
	}

	public virtual bool OnMessage(MessageSinkMessage message)
	{
		Messages.Add(message);
		return callback?.Invoke(message) ?? true;
	}
}

public class SpyMessageSink<TFinalMessage> : SpyMessageSink, IDisposable
{
	bool disposed;

	SpyMessageSink(Func<MessageSinkMessage, bool>? callback) :
		base(callback)
	{ }

	public ManualResetEvent Finished = new(initialState: false);

	public static SpyMessageSink<TFinalMessage> Create(Func<MessageSinkMessage, bool>? callback = null) =>
		new(callback);

	public void Dispose()
	{
		if (disposed)
			return;

		disposed = true;

		Finished.Dispose();
	}

	public override bool OnMessage(MessageSinkMessage message)
	{
		var result = base.OnMessage(message);

		if (message is TFinalMessage)
			Finished.Set();

		return result;
	}
}
