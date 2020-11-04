using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Adapts <see cref="IMessageSinkWithTypes"/> to <see cref="_IMessageSink"/>
	/// </summary>
	public static class Xunit2MessageSinkAdapter
	{
		/// <summary>
		/// Adapts a v3 message sink to support v2 messages. The v2 messages are
		/// automatically converted into their v3 form and passed along to the
		/// v3 message sink.
		/// </summary>
		/// <param name="v3MessageSink">The v3 message sink to adapt</param>
		/// <param name="adapter">The optional adapter (settable only for testing purposes)</param>
		/// <returns>A v2 message sink which supports both <see cref="IMessageSink"/>
		/// and <see cref="IMessageSinkWithTypes"/>.</returns>
		public static IMessageSinkWithTypes Adapt(
			_IMessageSink v3MessageSink,
			// TODO: Return type should eventually be _MessageSinkMessage
			Func<IMessageSinkMessage, HashSet<string>?, IMessageSinkMessage>? adapter = null) =>
				new V2toV3MessageSink(v3MessageSink, adapter);

		class V2toV3MessageSink : IMessageSink, IMessageSinkWithTypes
		{
			readonly _IMessageSink v3MessageSink;
			readonly Func<IMessageSinkMessage, HashSet<string>?, IMessageSinkMessage> adapter;

			public V2toV3MessageSink(
				_IMessageSink v3MessageSink,
				Func<IMessageSinkMessage, HashSet<string>?, IMessageSinkMessage>? adapter = null)
			{
				this.v3MessageSink = Guard.ArgumentNotNull(nameof(v3MessageSink), v3MessageSink);
				this.adapter = adapter ?? Xunit2MessageAdapter.Adapt;
			}

			public void Dispose()
			{
				(v3MessageSink as IDisposable)?.Dispose();
			}

			public bool OnMessage(IMessageSinkMessage message)
			{
				Guard.ArgumentNotNull(nameof(message), message);

				return OnMessageWithTypes(message, MessageSinkAdapter.GetImplementedInterfaces(message));
			}

			public bool OnMessageWithTypes(
				IMessageSinkMessage message,
				HashSet<string>? messageTypes)
			{
				Guard.ArgumentNotNull(nameof(message), message);

				var v3Message = adapter(message, messageTypes);
				return v3MessageSink.OnMessage(v3Message);
			}
		}
	}
}
