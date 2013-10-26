using System.Collections.Generic;

namespace Xunit.Sdk
{
    public interface ITestCaseOrderer
    {
        IEnumerable<XunitTestCase> OrderTestCases(IEnumerable<XunitTestCase> testCases);
    }
}
