using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;

public class AlphabeticalOrderer : ITestCaseOrderer 
{
    public IEnumerable<XunitTestCase> OrderTestCases(IEnumerable<XunitTestCase> testCases)
    {
        var result = testCases.ToList();
        result.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Method.Name, y.Method.Name));
        return result;
    }
}
