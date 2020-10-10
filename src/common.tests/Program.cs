using System;
using System.Threading.Tasks;

namespace Xunit.Runner.InProc.SystemConsole
{
	public class Program
	{
		// TODO: This should be (optionally?) auto-injected
		[STAThread]
		public static Task<int> Main(string[] args) =>
			ConsoleRunner.Run(args).AsTask();
	}
}
