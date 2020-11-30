namespace Xunit.v3
{
	/// <summary>
	/// An implementation of <see cref="_IMessageSink"/> that ignores all messages.
	/// </summary>
	public class _NullMessageSink : _IMessageSink
	{
		/// <inheritdoc/>
		public bool OnMessage(_MessageSinkMessage message) => true;
	}
}
