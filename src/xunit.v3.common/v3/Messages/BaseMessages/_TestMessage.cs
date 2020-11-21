using System;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// Base message for all messages related to tests.
	/// </summary>
	public class _TestMessage : _TestCaseMessage
	{
		string? testUniqueID;

		/// <summary>
		/// Gets the test's unique ID. Can be used to correlate test messages with the appropriate
		/// test that they're related to. Test metadata is provided as <see cref="_ITestMetadata"/>
		/// via <see cref="_TestStarting"/> (during execution) and should be cached if required.
		/// </summary>
		public string TestUniqueID
		{
			get => testUniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(TestUniqueID)} on an uninitialized '{GetType().FullName}' object");
			set => testUniqueID = Guard.ArgumentNotNullOrEmpty(nameof(TestUniqueID), value);
		}
	}
}
