using System.Collections.Generic;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// A class implements this interface to participate in ordering tests
	/// for the test runner. Test case orderers are applied using the
	/// <see cref="TestCaseOrdererAttribute"/>, which can be applied at
	/// the assembly, test collection, and test class level.
	/// </summary>
	public interface ITestCaseOrderer
	{
		/// <summary>
		/// Orders test cases for execution.
		/// </summary>
		/// <param name="testCases">The test cases to be ordered.</param>
		/// <returns>The test cases in the order to be run.</returns>
		IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
			where TTestCase : _ITestCase;
	}
}
