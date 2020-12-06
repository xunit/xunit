using Xunit.Abstractions;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Adapts <see cref="_IMessageSink"/> to a class which implements both <see cref="IMessageSink"/>
	/// and <see cref="IMessageSinkWithTypes"/>.
	/// </summary>
	public static class Xunit2MessageSinkAdapter
	{
		/// <summary>
		/// Adapts a v3 message sink to support v2 messages. The v2 messages are
		/// automatically converted into their v3 form and passed along to the
		/// v3 message sink.
		/// </summary>
		/// <param name="assemblyUniqueID">The unique ID of the assembly this adapter is for</param>
		/// <param name="v3MessageSink">The v3 message sink to adapt</param>
		/// <returns>A v2 message sink which supports both <see cref="IMessageSink"/>
		/// and <see cref="IMessageSinkWithTypes"/>.</returns>
		public static Xunit2MessageSink Adapt(
			string assemblyUniqueID,
			_IMessageSink v3MessageSink) =>
				new Xunit2MessageSink(assemblyUniqueID, v3MessageSink);
	}
}
