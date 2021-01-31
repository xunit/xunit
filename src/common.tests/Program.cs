using System;
using System.Threading.Tasks;

// This should only exist for our v3 test projects, not for general injection.
#if NETFRAMEWORK
[assembly: TestDriven.Framework.CustomTestRunner(typeof(Xunit.Runner.TdNet.TdNetRunner))]
#endif

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
