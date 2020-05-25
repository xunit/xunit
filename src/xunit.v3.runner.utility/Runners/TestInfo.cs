using System.Collections.Generic;

namespace Xunit.Runners
{
    /// <summary>
    /// A base class which contains information about a test.
    /// </summary>
    public abstract class TestInfo
    {
        /// <summary/>
        public TestInfo(string typeName,
                        string methodName,
                        Dictionary<string, List<string>> traits,
                        string testDisplayName,
                        string testCollectionDisplayName)
        {
            TypeName = typeName;
            MethodName = methodName;
            Traits = traits ?? new Dictionary<string, List<string>>();
            TestDisplayName = testDisplayName;
            TestCollectionDisplayName = testCollectionDisplayName;
        }

        /// <summary>
        /// The fully qualified type name of the class that contains the test.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// The name of the method that contains the test.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// The traits associated with the test.
        /// </summary>
        public Dictionary<string, List<string>> Traits { get; }

        /// <summary>
        /// The display name for the test.
        /// </summary>
        public string TestDisplayName { get; }

        /// <summary>
        /// The display name of the test collection the test belongs to.
        /// </summary>
        public string TestCollectionDisplayName { get; }
    }
}
