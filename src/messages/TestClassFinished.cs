using System.Collections.Generic;
using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
	/// <summary>
	/// Default implementation of <see cref="ITestClassFinished"/>.
	/// </summary>
	public class TestClassFinished : TestClassMessage, ITestClassFinished
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestClassFinished"/> class.
		/// </summary>
		public TestClassFinished(IEnumerable<ITestCase> testCases, ITestClass testClass, decimal executionTime, int testsRun, int testsFailed, int testsSkipped)
			: base(testCases, testClass)
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
