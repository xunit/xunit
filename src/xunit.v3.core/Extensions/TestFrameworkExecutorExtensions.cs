using System.Collections.Generic;
using System.Linq;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// Extension methods for <see cref="_ITestFrameworkExecutor"/>.
	/// </summary>
	public static class TestFrameworkExecutorExtensions
	{
		/// <summary>
		/// Starts the process of running selected tests in the assembly.
		/// </summary>
		/// <param name="executor">The test framework executor.</param>
		/// <param name="testCases">The test cases to run.</param>
		/// <param name="executionMessageSink">The message sink to report results back to.</param>
		/// <param name="executionOptions">The options to be used during test execution.</param>
		public static void RunTests(
			this _ITestFrameworkExecutor executor,
			IEnumerable<_TestCaseDiscovered> testCases,
			_IMessageSink executionMessageSink,
			_ITestFrameworkExecutionOptions executionOptions)
		{
			Guard.ArgumentNotNull(nameof(executor), executor);
			Guard.ArgumentNotNull(nameof(testCases), testCases);
			Guard.ArgumentNotNull(nameof(executionMessageSink), executionMessageSink);
			Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

			executor.RunTests(testCases.Select(tc => tc.Serialization), executionMessageSink, executionOptions);
		}
	}
}
