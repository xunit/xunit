using System;
using System.Threading.Tasks;

namespace Xunit.Runner.SystemConsole;

internal static class Program
{
	[STAThread]
	public static async Task<int> Main(string[] args)
	{
		using var runner = new ConsoleRunner(args);

		return await runner.EntryPoint();
	}
}
