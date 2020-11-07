using System;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// Base message interface for all messages related to test classes.
	/// </summary>
	public class _TestClassMessage : _MessageSinkMessage
	{
		string? testClassUniqueID;

		/// <summary>
		/// Gets the test class's unique ID. Can be used to correlate test messages with the appropriate
		/// test class that they're related to. Test class metadata (as <see cref="_ITestClassMetadata"/>)
		/// is provided via <see cref="_TestClassStarting"/> (during execution) and should be cached as needed.
		/// </summary>
		public string TestClassUniqueID
		{
			get => testClassUniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(TestClassUniqueID)} on an uninitialized '{GetType().FullName}' object");
			set => testClassUniqueID = Guard.ArgumentNotNullOrEmpty(nameof(TestClassUniqueID), value);
		}
	}
}
