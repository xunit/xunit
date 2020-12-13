using System.Collections.Generic;

namespace Xunit.v3
{
	/// <summary>
	/// Represents a single test case in the system. This test case usually represents a single test, but in
	/// the case of dynamically generated data for data driven tests, the test case may actually return
	/// multiple results when run.
	/// </summary>
	public interface _ITestCase
	{
		/// <summary>
		/// Gets the display name of the test case.
		/// </summary>
		string DisplayName { get; }

		/// <summary>
		/// Gets the display text for the reason a test is being skipped; if the test
		/// is not skipped, returns <c>null</c>.
		/// </summary>
		string? SkipReason { get; }

		/// <summary>
		/// Get or sets the source file name and line where the test is defined, if requested (and known).
		/// </summary>
		_ISourceInformation? SourceInformation { get; set; }

		/// <summary>
		/// Gets the test method this test case belongs to.
		/// </summary>
		_ITestMethod TestMethod { get; }

		/// <summary>
		/// Gets the arguments that will be passed to the test method.
		/// </summary>
		object?[]? TestMethodArguments { get; }

		/// <summary>
		/// Gets the trait values associated with this test case. If
		/// there are none, or the framework does not support traits,
		/// this should return an empty dictionary (not <c>null</c>). This
		/// dictionary must be treated as read-only.
		/// </summary>
		Dictionary<string, List<string>> Traits { get; }

		/// <summary>
		/// Gets a unique identifier for the test case.
		/// </summary>
		/// <remarks>
		/// The unique identifier for a test case should be able to discriminate
		/// among test cases, even those which are varied invocations against the
		/// same test method (i.e., theories). Ideally, this identifier would remain
		/// stable until such time as the developer changes some fundamental part
		/// of the identity (assembly, class name, test name, or test data); however,
		/// the minimum stability of the identifier must at least extend across
		/// multiple discoveries of the same test in the same (non-recompiled)
		/// assembly.
		/// </remarks>
		string UniqueID { get; }
	}
}
