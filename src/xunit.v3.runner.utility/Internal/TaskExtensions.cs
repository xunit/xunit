using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.Internal;

[ExcludeFromCodeCoverage]
internal static class TaskExtensions
{
	public static bool SpinWait(
		this Task task,
		int milliseconds)
	{
		var spin = default(SpinWait);
		var stopwatch = Stopwatch.StartNew();

		while (true)
		{
			if (task.IsCompleted)
				return true;

			if (stopwatch.ElapsedMilliseconds > milliseconds)
				return false;

			spin.SpinOnce();
		}
	}
}
