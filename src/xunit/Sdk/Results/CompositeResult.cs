using System;
using System.Collections.Generic;

namespace Xunit.Sdk
{
    /// <summary>
    /// Contains multiple test results, representing them as a composite test result.
    /// </summary>
    [Serializable]
    public abstract class CompositeResult : TestResult
    {
        readonly List<ITestResult> results = new List<ITestResult>();

        /// <summary>
        /// Gets the test results.
        /// </summary>
        public IList<ITestResult> Results
        {
            get { return results.AsReadOnly(); }
        }

        /// <summary>
        /// Adds a test result to the composite test result list.
        /// </summary>
        /// <param name="testResult"></param>
        public virtual void Add(ITestResult testResult)
        {
            results.Add(testResult);
        }
    }
}