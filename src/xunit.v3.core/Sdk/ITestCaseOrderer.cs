using System.Collections.Generic;
using Xunit.Abstractions;

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
        IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase;
    }
}
