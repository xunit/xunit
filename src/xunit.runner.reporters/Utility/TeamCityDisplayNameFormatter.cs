using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class TeamCityDisplayNameFormatter
    {
        int assemblyCount;
        readonly Dictionary<string, int> assemblyMappings = new Dictionary<string, int>();

        public virtual string DisplayName(ITestCollection testCollection)
        {
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

        public virtual string DisplayName(ITest test)
        {
            return test.DisplayName;
        }
    }
}
