using System;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// Base message for all messages related to test cases.
	/// </summary>
	public class _TestCaseMessage : _TestMethodMessage
	{
		string? testCaseUniqueID;

		/// <summary>
		/// Gets the test case's unique ID. Can be used to correlate test messages with the appropriate
		/// test case that they're related to. Test case metadata is provided as <see cref="_ITestCaseMetadata"/>
		/// via <see cref="_TestCaseStarting"/> (during execution) or <see cref="_TestCaseDiscovered"/>
		/// (during discovery) and should be cached if required.
		/// </summary>
		public string TestCaseUniqueID
		{
			get => testCaseUniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(TestCaseUniqueID)} on an uninitialized '{GetType().FullName}' object");
			set => testCaseUniqueID = Guard.ArgumentNotNullOrEmpty(nameof(TestCaseUniqueID), value);
		}
	}
}
