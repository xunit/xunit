namespace Xunit.v3
{
	/// <summary>
	/// An implementation of <see cref="_IMessageSink"/> that ignores all messages.
	/// </summary>
	public class _NullMessageSink : _IMessageSink
	{
		/// <inheritdoc/>
		public bool OnMessage(_MessageSinkMessage message) => true;

		/// <inheritdoc/>
		public override bool Equals(object? obj)
		{
			return (obj is _NullMessageSink);
		}

		/// <inheritdoc/>
		public bool Equals(_NullMessageSink other)
		{
			return (other is _NullMessageSink);
		}

		/// <inheritdoc/>
		public static bool operator ==(_NullMessageSink lhs, _NullMessageSink rhs)
		{
			return (lhs is _NullMessageSink && rhs is _NullMessageSink);
		}

		/// <inheritdoc/>
		public static bool operator !=(_NullMessageSink lhs, _NullMessageSink rhs)
		{
			return !(lhs == rhs);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return typeof(_NullMessageSink).GetHashCode();
		}
	}
}
