using System;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// Base message for all messages related to test collections.
	/// </summary>
	public class _TestCollectionMessage : _TestAssemblyMessage
	{
		string? testCollectionUniqueID;

		/// <summary>
		/// Gets the test collection's unique ID. Can be used to correlate test messages with the appropriate
		/// test collection that they're related to. Test collection metadata (as <see cref="_ITestCollectionMetadata"/>)
		/// is provided via <see cref="_TestCollectionStarting"/> (during execution) and should be cached as needed.
		/// </summary>
		public string TestCollectionUniqueID
		{
			get => testCollectionUniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(TestCollectionUniqueID)} on an uninitialized '{GetType().FullName}' object");
			set => testCollectionUniqueID = Guard.ArgumentNotNullOrEmpty(nameof(TestCollectionUniqueID), value);
		}
	}
}
