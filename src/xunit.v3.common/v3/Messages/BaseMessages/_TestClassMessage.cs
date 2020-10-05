// TODO: Can we/should we figure out a unique ID for test classes? Is the name enough?

namespace Xunit.v3
{
	/// <summary>
	/// Base message interface for all messages related to test classes.
	/// </summary>
	public class _TestClassMessage : _TestCollectionMessage
	{
		/// <summary>
		/// The fully qualified type name of the test class that is associated with this message.
		/// If there is no test class, then this returns <c>null</c>.
		/// </summary>
		public string? TestClass { get; set; }
	}
}
