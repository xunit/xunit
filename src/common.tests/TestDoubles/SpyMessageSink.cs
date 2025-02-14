using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Sdk;

public class SpyMessageSink : IMessageSink
{
	protected SpyMessageSink(Func<IMessageSinkMessage, bool>? callback = null) =>
		Callback = callback ?? ((msg) => true);

	public Func<IMessageSinkMessage, bool> Callback { get; set; }

	public List<IMessageSinkMessage> Messages { get; } = [];

	public static SpyMessageSink Capture(Func<IMessageSinkMessage, bool>? callback = null) =>
		new(callback);

	public static IMessageSink Create(
		bool returnResult = true,
		List<IMessageSinkMessage>? messages = null) =>
			Create(_ => returnResult, messages);

	public static IMessageSink Create(
		Func<IMessageSinkMessage, bool> lambda,
		List<IMessageSinkMessage>? messages = null) =>
			new CaptureMessageSink(lambda, messages);

	public virtual bool OnMessage(IMessageSinkMessage message)
	{
		Messages.Add(message);
		return Callback?.Invoke(message) ?? true;
	}

	class CaptureMessageSink(
		Func<IMessageSinkMessage, bool> lambda,
		List<IMessageSinkMessage>? messages) :
			IMessageSink
	{
		public bool OnMessage(IMessageSinkMessage message)
		{
			messages?.Add(message);
			return lambda(message);
		}
	}
}

public sealed class SpyMessageSink<TFinalMessage> : SpyMessageSink, IDisposable
{
	bool disposed;

	SpyMessageSink(Func<IMessageSinkMessage, bool>? callback) :
		base(callback)
	{ }

	public ManualResetEvent Finished = new(initialState: false);

	public static SpyMessageSink<TFinalMessage> Create(Func<IMessageSinkMessage, bool>? callback = null) =>
		new(callback);

	public void Dispose()
	{
		if (disposed)
			return;

		disposed = true;

		Finished.Dispose();
	}

	public override bool OnMessage(IMessageSinkMessage message)
	{
		var result = base.OnMessage(message);

		if (message is TFinalMessage)
			Finished.Set();

		return result;
	}
}
