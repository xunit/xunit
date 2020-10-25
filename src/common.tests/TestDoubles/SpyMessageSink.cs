using System;
using System.Collections.Generic;
using System.Threading;
using NSubstitute;
using Xunit.Abstractions;

namespace Xunit.v3
{
	public static class SpyMessageSink
	{
		public static IMessageSink Create(
			bool returnResult = true,
			List<IMessageSinkMessage>? messages = null) =>
				Create(_ => returnResult, messages);

		public static IMessageSink Create(
			Func<IMessageSinkMessage, bool> lambda,
			List<IMessageSinkMessage>? messages = null)
		{
			var result = Substitute.For<IMessageSink>();

			result
				.OnMessage(null)
				.ReturnsForAnyArgs(callInfo =>
				{
					var message = callInfo.Arg<IMessageSinkMessage>();

					if (messages != null)
						messages.Add(message);

					return lambda(message);
				});

			return result;
		}
	}

	public class SpyMessageSink<TFinalMessage> : IMessageSink, IDisposable
	{
		readonly Func<IMessageSinkMessage, bool> cancellationThunk;
		bool disposed;

		SpyMessageSink(Func<IMessageSinkMessage, bool>? cancellationThunk)
		{
			this.cancellationThunk = cancellationThunk ?? (msg => true);
		}

		public ManualResetEvent Finished = new ManualResetEvent(initialState: false);

		public List<IMessageSinkMessage> Messages = new List<IMessageSinkMessage>();

		public static SpyMessageSink<TFinalMessage> Create(Func<IMessageSinkMessage, bool>? cancellationThunk = null) =>
			new SpyMessageSink<TFinalMessage>(cancellationThunk);

		public void Dispose()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);

			disposed = true;

			Finished.Dispose();
		}

		public bool OnMessage(IMessageSinkMessage message)
		{
			Messages.Add(message);

			if (message is TFinalMessage)
				Finished.Set();

			return cancellationThunk(message);
		}
	}
}
