using Xunit.Abstractions;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="ITestCaseDiscoveryMessage"/>.
	/// </summary>
	public class TestCaseDiscoveryMessage : TestCaseMessage, ITestCaseDiscoveryMessage
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestCaseDiscoveryMessage"/> class.
		/// </summary>
		public TestCaseDiscoveryMessage(ITestCase testCase)
			: base(testCase)
		{ }
	}
}
