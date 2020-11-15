using System.Collections.Generic;

namespace Xunit.v3
{
	/// <summary>
	/// Represents metadata about a test case.
	/// </summary>
	public interface _ITestCaseMetadata
	{
		/// <summary>
		/// Gets the display text for the reason a test is being skipped; if the test
		/// is not skipped, returns <c>null</c>.
		/// </summary>
		string? SkipReason { get; }

		/// <summary>
		/// Gets the source file name. A <c>null</c> value indicates that the
		/// source file name is not known.
		/// </summary>
		string? SourceFilePath { get; }

		/// <summary>
		/// Gets the source file line number. A <c>null</c> value indicates that the
		/// source file line number is not known.
		/// </summary>
		int? SourceLineNumber { get; }

		/// <summary>
		/// Gets the display name of the test case.
		/// </summary>
		string TestCaseDisplayName { get; }

		/// <summary>
		/// Gets the trait values associated with this test case. If
		/// there are none, or the framework does not support traits,
		/// this should return an empty dictionary (not <c>null</c>).
		/// </summary>
		public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits { get; }
	}
}
