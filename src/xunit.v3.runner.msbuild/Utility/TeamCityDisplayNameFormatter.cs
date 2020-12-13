using System.Collections.Concurrent;
using System.Threading;
using Xunit.v3;

namespace Xunit
{
	public class TeamCityDisplayNameFormatter
	{
		int assemblyCount;
		readonly ConcurrentDictionary<string, int> assemblyMappings = new ConcurrentDictionary<string, int>();

		public virtual string DisplayName(_ITestCollection testCollection)
		{
			var id = assemblyMappings.GetOrAdd(
				testCollection.TestAssembly.Assembly.Name,
				key => Interlocked.Increment(ref assemblyCount)
			);

			return string.Concat(testCollection.DisplayName, " (", id, ")");
		}

		public virtual string DisplayName(_ITest test)
		{
			return test.DisplayName;
		}
	}
}
