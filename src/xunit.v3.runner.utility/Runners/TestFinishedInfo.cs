using System.Collections.Generic;

namespace Xunit.Runners
{
    /// <summary>
    /// Represents a test that finished, regardless of the result.
    /// </summary>
    public class TestFinishedInfo : TestExecutedInfo
    {
        /// <summary/>
        public TestFinishedInfo(string typeName,
                                string methodName,
                                Dictionary<string, List<string>> traits,
                                string testDisplayName,
                                string testCollectionDisplayName,
                                decimal executionTime,
                                string output)
            : base(typeName, methodName, traits, testDisplayName, testCollectionDisplayName, executionTime, output)
        { }
    }
}
