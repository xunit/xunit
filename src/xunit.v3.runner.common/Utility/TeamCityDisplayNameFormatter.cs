using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Runner.Common
{
    /// <summary>
    /// A class which can generate display names for test collections and tests.
    /// </summary>
    public class TeamCityDisplayNameFormatter
    {
        int assemblyCount;
        readonly Dictionary<string, int> assemblyMappings = new Dictionary<string, int>();

        /// <summary>
        /// Get the unique display name for a test collection. Since test collections are not required to
        /// have unique names (across assemblies), automatically appends an incrementing ID to the name.
        /// </summary>
        /// <param name="testCollection">The test collection to get the display name for</param>
        /// <returns>The unique display name for the test collection</returns>
        public virtual string DisplayName(ITestCollection testCollection)
        {
            Guard.ArgumentNotNull(nameof(testCollection), testCollection);

            int id;

            lock (assemblyMappings)
            {
                if (!assemblyMappings.TryGetValue(testCollection.TestAssembly.Assembly.Name, out id))
                {
                    id = Interlocked.Increment(ref assemblyCount);
                    assemblyMappings[testCollection.TestAssembly.Assembly.Name] = id;
                }
            }

            return $"{testCollection.DisplayName} ({id})";
        }

        /// <summary>
        /// Gets the display name for a test.
        /// </summary>
        /// <param name="test">The test to get the display name for.</param>
        /// <returns>The display name of the test.</returns>
        public virtual string DisplayName(ITest test)
        {
            Guard.ArgumentNotNull(nameof(test), test);

            return test.DisplayName;
        }
    }
}
