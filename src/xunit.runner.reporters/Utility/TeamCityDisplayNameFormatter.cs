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

            return EscapeValue($"{testCollection.DisplayName}");
        }

        public virtual string DisplayName(ITest test)
        {
            return test.DisplayName;
        }

        static string EscapeValue(string value)
        {
            value = value.Replace("|", "||");
            value = value.Replace("'", "|'");
            value = value.Replace("[", "|[");
            value = value.Replace("]", "|]");
            value = value.Replace("\r", "|r");
            value = value.Replace("\n", "|n");

            return value;
        }
    }
}
