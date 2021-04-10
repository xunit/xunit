using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;
using Xunit.v3;

public class MockTestCaseOrderer : ITestCaseOrderer
{
	private readonly bool reverse;

	public MockTestCaseOrderer(bool reverse = false)
	{
		this.reverse = reverse;
	}

	public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
		where TTestCase : _ITestCase
	{
		if (!reverse)
			return testCases;

		var result = testCases.ToList();
		result.Reverse();
		return result;
	}
}
