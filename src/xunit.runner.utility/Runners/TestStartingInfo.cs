using System.Collections.Generic;

namespace Xunit.Runners
{
    /// <summary>
    /// Represents a test that is starting.
    /// </summary>
    public class TestStartingInfo : TestInfo
    {
        /// <summary/>
        public TestStartingInfo(string typeName,
                              string methodName,
                              Dictionary<string, List<string>> traits,
                              string testDisplayName,
                              string testCollectionDisplayName)
            : base(typeName, methodName, traits, testDisplayName, testCollectionDisplayName)
        { }
    }
}
