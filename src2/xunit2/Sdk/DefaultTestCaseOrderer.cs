using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class DefaultTestCaseOrderer : ITestCaseOrderer
    {
        public IEnumerable<XunitTestCase> OrderTestCases(IEnumerable<XunitTestCase> testCases)
        {
            var result = testCases.ToList();
            result.Sort(Compare);
            return result;
        }

        int Compare(ITestCase x, ITestCase y)
        {
            var xHash = x.UniqueID.GetHashCode();
            var yHash = y.UniqueID.GetHashCode();

            if (xHash == yHash)
                return 0;
            if (xHash < yHash)
                return -1;
            return 1;
        }
    }
}
