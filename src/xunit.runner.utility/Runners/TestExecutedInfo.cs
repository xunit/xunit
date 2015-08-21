using System.Collections.Generic;

namespace Xunit.Runners
{
    /// <summary>
    /// Represents information about a test that was executed.
    /// </summary>
    public abstract class TestExecutedInfo : TestInfo
    {
        /// <summary/>
        public TestExecutedInfo(string typeName,
                                string methodName,
                                Dictionary<string, List<string>> traits,
                                string testDisplayName,
                                string testCollectionDisplayName,
                                decimal executionTime,
                                string output)
            : base(typeName, methodName, traits, testDisplayName, testCollectionDisplayName)
        {
            ExecutionTime = executionTime;
            Output = output ?? string.Empty;
        }

        /// <summary>
        /// The number of seconds the test spent executing.
        /// </summary>
        public decimal ExecutionTime { get; }

        /// <summary>
        /// The output from the test.
        /// </summary>
        public string Output { get; }
    }
}
