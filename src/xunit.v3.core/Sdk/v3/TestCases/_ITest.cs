namespace Xunit.v3
{
	/// <summary>
	/// Represents a single test in the system. A test case typically contains only a single test,
	/// but may contain many if circumstances warrant it (for example, test data for a theory cannot
	/// be pre-enumerated, so the theory yields a single test case with multiple tests).
	/// </summary>
	public interface _ITest
	{
		/// <summary>
		/// Gets the display name of the test.
		/// </summary>
		string DisplayName { get; }

		/// <summary>
		/// Gets the test case this test belongs to.
		/// </summary>
		_ITestCase TestCase { get; }

		/// <summary>
		/// Gets a unique identifier for the test.
		/// </summary>
		string UniqueID { get; }
	}
}
