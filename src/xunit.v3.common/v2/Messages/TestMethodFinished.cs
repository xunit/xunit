using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="ITestMethodFinished"/>.
	/// </summary>
	public class TestMethodFinished : TestMethodMessage, ITestMethodFinished
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestMethodFinished"/> class.
		/// </summary>
		public TestMethodFinished(
			IEnumerable<ITestCase> testCases,
			ITestMethod testMethod,
			decimal executionTime,
			int testsRun,
			int testsFailed,
			int testsSkipped)
				: base(testCases, testMethod)
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
