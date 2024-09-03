using System;
using System.Threading.Tasks;

namespace Xunit.Runner.SystemConsole;

internal static class Program
{
	[STAThread]
	public static Task<int> Main(string[] args) =>
		new ConsoleRunner(args).EntryPoint().AsTask();
}
