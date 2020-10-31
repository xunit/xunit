using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// An implementation of <see cref="IMessageSink"/> and <see cref="IMessageSinkWithTypes"/>
	/// that ignores all messages.
	/// </summary>
	public class NullMessageSink : LongLivedMarshalByRefObject, IMessageSink, IMessageSinkWithTypes
	{
		/// <inheritdoc/>
		public void Dispose()
		{ }

		/// <inheritdoc/>
		public bool OnMessage(IMessageSinkMessage message) => true;

		/// <inheritdoc/>
		public bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string>? messageTypes) => true;
	}
}
