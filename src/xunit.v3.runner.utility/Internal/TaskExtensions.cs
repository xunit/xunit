using System.Diagnostics;

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
