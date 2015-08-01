using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestCaseOrderer"/>. Orders tests in
    /// an unpredictable but stable order, so that repeated test runs of the
    /// identical test assembly run tests in the same order.
    /// </summary>
    public class DefaultTestCaseOrderer : ITestCaseOrderer
    {
        /// <inheritdoc/>
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
        {
            var result = testCases.ToList();
            result.Sort(Compare);
            return result;
        }

        int Compare<TTestCase>(TTestCase x, TTestCase y)
            where TTestCase : ITestCase
        {
            Guard.ArgumentNotNull(nameof(x), x);
            Guard.ArgumentNotNull(nameof(y), y);

            if (x.UniqueID == null)
                throw new ArgumentException($"Could not compare test case {x.DisplayName} because it has a null UniqueID");
            if (y.UniqueID == null)
                throw new ArgumentException($"Could not compare test case {y.DisplayName} because it has a null UniqueID");

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
