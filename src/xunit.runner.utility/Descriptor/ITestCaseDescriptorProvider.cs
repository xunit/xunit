using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// INTERNAL INTERFACE. DO NOT USE.
    /// </summary>
    public interface ITestCaseDescriptorProvider
    {
        /// <summary/>
        List<TestCaseDescriptor> GetTestCaseDescriptors(List<ITestCase> testCases, bool includeSerialization);
    }
}
