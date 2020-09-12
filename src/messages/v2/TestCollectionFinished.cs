using System.Collections.Generic;
using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.v2
#else
namespace Xunit.Runner.v2
#endif
{
	/// <summary>
	/// Default implementation of <see cref="ITestCollectionFinished"/>.
	/// </summary>
	public class TestCollectionFinished : TestCollectionMessage, ITestCollectionFinished
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestCollectionFinished"/> class.
		/// </summary>
		public TestCollectionFinished(
			IEnumerable<ITestCase> testCases,
			ITestCollection testCollection,
			decimal executionTime,
			int testsRun,
			int testsFailed,
			int testsSkipped)
				: base(testCases, testCollection)
		{
			ExecutionTime = executionTime;
			TestsRun = testsRun;
			TestsFailed = testsFailed;
			TestsSkipped = testsSkipped;
		}

		/// <inheritdoc/>
		public decimal ExecutionTime { get; }

		/// <inheritdoc/>
		public int TestsFailed { get; }

		/// <inheritdoc/>
		public int TestsRun { get; }

		/// <inheritdoc/>
		public int TestsSkipped { get; }
	}
}
