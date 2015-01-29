using System;
using System.Collections.Concurrent;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Runner.MSBuild
{
    public class TeamCityDisplayNameFormatter
    {
        readonly ConcurrentDictionary<string, int> assemblyMappings = new ConcurrentDictionary<string, int>();
        int assemblyCount = 0;

        public virtual string DisplayName(ITestCollection testCollection)
        {
            var id = assemblyMappings.GetOrAdd(
                testCollection.TestAssembly.Assembly.Name,
                key => Interlocked.Increment(ref assemblyCount));

            return String.Concat(testCollection.DisplayName, " (", id, ")");
        }

        public virtual string DisplayName(ITest test)
        {
            return test.DisplayName;
        }
    }
}