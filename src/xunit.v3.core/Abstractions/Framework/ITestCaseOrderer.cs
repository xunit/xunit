using System.Collections.Generic;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A class implements this interface to participate in ordering tests for the test runner.
/// Test case orderers are applied using an implementation of <see cref="ITestCaseOrdererAttribute"/>
/// (most commonly <see cref="TestCaseOrdererAttribute"/>), which can be applied at the assembly,
/// test collection, and test class level.
/// </summary>
public interface ITestCaseOrderer
{
	/// <summary>
	/// Orders test cases for execution.
	/// </summary>
	/// <param name="testCases">The test cases to be ordered.</param>
	/// <returns>The test cases in the order to be run.</returns>
	IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
		where TTestCase : notnull, ITestCase;
}
