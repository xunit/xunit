using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="ITestCaseMessage"/>.
	/// </summary>
	public class TestCaseMessage : TestMethodMessage, ITestCaseMessage
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestCaseMessage"/> class.
		/// </summary>
		public TestCaseMessage(ITestCase testCase)
			: base(testCase, testCase.TestMethod)
		{ }

		/// <inheritdoc/>
		public ITestCase TestCase => TestCases.FirstOrDefault();
	}
}
