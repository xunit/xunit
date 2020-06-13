using System;
using System.Linq;

namespace Xunit.Runner.SystemConsole
{
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            var consoleLock = new object();
            return new ConsoleRunner(consoleLock).EntryPoint(args);
        }
    }
}
