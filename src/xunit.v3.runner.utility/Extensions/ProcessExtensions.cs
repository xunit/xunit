using System.Diagnostics;
using System.Threading.Tasks;

namespace Xunit.Runner.v3.Extensions
{
	internal static class ProcessExtensions
	{
		/// <summary>Await a specified number of milliseconds for a process to exit.</summary>
		/// <param name="process">The process to await for</param>
		/// <param name="milliseconds">The amount of time, in milliseconds, to await for the associated process to exit. The maximum is the largest possible value of a 32-bit integer, which represents infinity to the operating system.</param>
		/// <returns>
		/// <see langword="true" /> if the associated process has exited; otherwise, <see langword="false" />.</returns>
		public static async ValueTask<bool> WaitForExitAsync(this Process process, int milliseconds)
		{
			if (process.HasExited)
			{
				return true;
			}
			var tcs = new TaskCompletionSource<int>(); // int is used as a don't care type
			process.EnableRaisingEvents = true;
			process.Exited += (s, e) => tcs.TrySetResult(default);
			_ = await Task.WhenAny(tcs.Task, Task.Delay(milliseconds));
			return process.HasExited;
		}
	}
}
