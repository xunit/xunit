using System.Collections.Generic;

namespace Xunit.Runners
{
    /// <summary>
    /// Represents a test that passed.
    /// </summary>
    public class TestPassedInfo : TestExecutedInfo
    {
        /// <summary/>
        public TestPassedInfo(string typeName,
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
