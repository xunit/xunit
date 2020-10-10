using System;
using System.Threading.Tasks;

namespace Xunit.Runner.SystemConsole
{
	public class Program
	{
		[STAThread]
		public static Task<int> Main(string[] args) =>
			new ConsoleRunner(args).EntryPoint().AsTask();
	}
}
