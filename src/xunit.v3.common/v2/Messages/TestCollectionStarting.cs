using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="ITestCollectionStarting"/>.
	/// </summary>
	public class TestCollectionStarting : TestCollectionMessage, ITestCollectionStarting
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestCollectionStarting"/> class.
		/// </summary>
		public TestCollectionStarting(
			IEnumerable<ITestCase> testCases,
			ITestCollection testCollection)
				: base(testCases, testCollection)
		{ }
	}
}
