using System.Collections.Generic;
using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.v2
#else
namespace Xunit.Runner.v2
#endif
{
	/// <summary>
	/// Default implementation of <see cref="ITestMethodStarting"/>.
	/// </summary>
	public class TestMethodStarting : TestMethodMessage, ITestMethodStarting
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestMethodStarting"/> class.
		/// </summary>
		public TestMethodStarting(
			IEnumerable<ITestCase> testCases,
			ITestMethod testMethod)
				: base(testCases, testMethod)
		{ }
	}
}
