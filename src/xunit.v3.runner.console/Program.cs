using System;

namespace Xunit.Runner.SystemConsole
{
	public class Program
	{
		[STAThread]
		public static int Main(string[] args) =>
			new ConsoleRunner(args).EntryPoint();
	}
}
