using Xunit.Sdk;
using Xunit.v3;

public class MockTestCaseOrderer(bool reverse = false) :
	ITestCaseOrderer
{
	public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
		where TTestCase : notnull, ITestCase
	{
		if (!reverse)
			return testCases;

		var result = testCases.ToList();
		result.Reverse();
		return result;
	}
}
