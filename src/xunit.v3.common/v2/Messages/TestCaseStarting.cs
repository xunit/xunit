using Xunit.Abstractions;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="ITestCaseStarting"/>.
	/// </summary>
	public class TestCaseStarting : TestCaseMessage, ITestCaseStarting
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestCaseStarting"/> class.
		/// </summary>
		public TestCaseStarting(ITestCase testCase)
			: base(testCase)
		{ }
	}
}
