using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

public class MockTestCaseOrderer : ITestCaseOrderer
{
    private readonly bool reverse;

    public MockTestCaseOrderer(bool reverse = false)
    {
        this.reverse = reverse;
    }

    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
        where TTestCase : ITestCase
    {
        if (!reverse)
            return testCases;

        var result = testCases.ToList();
        result.Reverse();
        return result;
    }
}
