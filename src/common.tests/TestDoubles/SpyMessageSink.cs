using System;
using System.Collections.Generic;
using System.Threading;
using NSubstitute;

namespace Xunit.v3
{
	public static class SpyMessageSink
	{
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

					if (messages != null)
						messages.Add(message);

					return lambda(message);
				});

			return result;
		}
	}

	public class SpyMessageSink<TFinalMessage> : _IMessageSink, IDisposable
	{
		readonly Func<_MessageSinkMessage, bool> cancellationThunk;
		bool disposed;

		SpyMessageSink(Func<_MessageSinkMessage, bool>? cancellationThunk)
		{
			this.cancellationThunk = cancellationThunk ?? (msg => true);
		}

		public ManualResetEvent Finished = new ManualResetEvent(initialState: false);

		public List<_MessageSinkMessage> Messages = new List<_MessageSinkMessage>();

		public static SpyMessageSink<TFinalMessage> Create(Func<_MessageSinkMessage, bool>? cancellationThunk = null) =>
			new SpyMessageSink<TFinalMessage>(cancellationThunk);

		public void Dispose()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);

			disposed = true;

			Finished.Dispose();
		}

		public bool OnMessage(_MessageSinkMessage message)
		{
			Messages.Add(message);

			if (message is TFinalMessage)
				Finished.Set();

			return cancellationThunk(message);
		}
	}
}
