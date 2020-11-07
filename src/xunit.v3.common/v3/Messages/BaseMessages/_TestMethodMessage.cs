using System;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// Base message for all messages related to test methods.
	/// </summary>
	public class _TestMethodMessage : _MessageSinkMessage
	{
		string? testMethodUniqueID;

		/// <summary>
		/// Gets the test method's unique ID. Can be used to correlate test messages with the appropriate
		/// test method that they're related to. Test method metadata (as <see cref="_ITestMethodMetadata"/>)
		/// is provided via <see cref="_TestMethodStarting"/> (during execution) and should be cached as needed.
		/// </summary>
		public string TestMethodUniqueID
		{
			get => testMethodUniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(TestMethodUniqueID)} on an uninitialized '{GetType().FullName}' object");
			set => testMethodUniqueID = Guard.ArgumentNotNullOrEmpty(nameof(TestMethodUniqueID), value);
		}
	}
}
