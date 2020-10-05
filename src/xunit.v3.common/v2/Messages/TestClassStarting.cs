using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="ITestClassStarting"/>.
	/// </summary>
	public class TestClassStarting : TestClassMessage, ITestClassStarting
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestClassStarting"/> class.
		/// </summary>
		public TestClassStarting(
			IEnumerable<ITestCase> testCases,
			ITestClass testClass)
				: base(testCases, testClass)
		{ }
	}
}
