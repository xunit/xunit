using System.Collections.Generic;

namespace Xunit
{
    /// <summary>
    /// Represents a single test method.
    /// </summary>
    public class TestMethod
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestMethod"/> class.
        /// </summary>
        /// <param name="methodName">The method name.</param>
        /// <param name="displayName">The method's display name.</param>
        /// <param name="traits">The method's traits.</param>
        public TestMethod(string methodName, string displayName, MultiValueDictionary<string, string> traits)
        {
            MethodName = methodName;
            DisplayName = displayName;
            Traits = traits ?? new MultiValueDictionary<string, string>();
            RunResults = new List<TestResult>();
        }

        /// <summary>
        /// Gets the method's display name.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets the method's name.
        /// </summary>
        public string MethodName { get; private set; }

        /// <summary>
        /// Gets the run results for the last run.
        /// </summary>
        public List<TestResult> RunResults { get; internal set; }

        /// <summary>
        /// Gets the composite run status for all the results of the last run.
        /// </summary>
        public TestStatus RunStatus
        {
            get
            {
                TestStatus result = TestStatus.NotRun;

                foreach (TestResult testResult in RunResults)
                    if (testResult is TestPassedResult)
                    {
                        if (result == TestStatus.NotRun)
                            result = TestStatus.Passed;
                    }
                    else if (testResult is TestSkippedResult)
                    {
                        if (result != TestStatus.Failed)
                            result = TestStatus.Skipped;
                    }
                    else if (testResult is TestFailedResult)
                    {
                        result = TestStatus.Failed;
                    }

                return result;
            }
        }

        /// <summary>
        /// Gets the test class this test method belongs to.
        /// </summary>
        public TestClass TestClass { get; internal set; }

        /// <summary>
        /// Gets the method's traits.
        /// </summary>
        public MultiValueDictionary<string, string> Traits { get; private set; }
    }
}
