using System.Collections.Generic;

namespace Xunit.Runners
{
    /// <summary>
    /// Represents a test that was skipped.
    /// </summary>
    public class TestSkippedInfo : TestInfo
    {
        /// <summary/>
        public TestSkippedInfo(string typeName,
                               string methodName,
                               Dictionary<string, List<string>> traits,
                               string testDisplayName,
                               string testCollectionDisplayName,
                               string skipReason)
            : base(typeName, methodName, traits, testDisplayName, testCollectionDisplayName)
        {
            SkipReason = skipReason;
        }

        /// <summary>
        /// Gets the reason that was given for skipping the test.
        /// </summary>
        public string SkipReason { get; }
    }
}
