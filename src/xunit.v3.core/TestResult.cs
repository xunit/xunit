namespace Xunit
{
	/// <summary>
	/// Indicates the result of running the test.
	/// </summary>
	public enum TestResult
	{
		/// <summary>
		/// The test passed.
		/// </summary>
		Passed,

		/// <summary>
		/// The test failed.
		/// </summary>
		Failed,

		/// <summary>
		/// The test was skipped.
		/// </summary>
		Skipped,
	}
}
