using System.Collections.Generic;

namespace Xunit.Runners
{
    /// <summary>
    /// Represents live test output.
    /// </summary>
    public class TestOutputInfo : TestInfo
    {
        /// <summary/>
        public TestOutputInfo(string typeName,
                              string methodName,
                              Dictionary<string, List<string>> traits,
                              string testDisplayName,
                              string testCollectionDisplayName,
                              string output)
            : base(typeName, methodName, traits, testDisplayName, testCollectionDisplayName)
        {
            Output = output;
        }

        /// <summary>
        /// The output from the test.
        /// </summary>
        public string Output { get; }
    }
}
