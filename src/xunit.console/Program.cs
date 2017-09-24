using System;
using System.IO;

namespace Xunit.ConsoleClient
{
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
#if NETCOREAPP1_0 || NETCOREAPP2_0
            var packagesPath = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
            if (string.IsNullOrEmpty(packagesPath))
            {
                packagesPath = Environment.GetEnvironmentVariable("USERPROFILE");
                if (string.IsNullOrEmpty(packagesPath))
                    packagesPath = Environment.GetEnvironmentVariable("HOME");

                if (string.IsNullOrEmpty(packagesPath))
                {
                    Console.WriteLine("error: user must set USERPROFILE or HOME environment variable");
                    return -1;
                }

                packagesPath = Path.Combine(packagesPath, ".nuget", "packages");
            }

            using (NetCoreAssemblyHelper.SubscribeResolve(packagesPath))
#endif
            {
                return new ConsoleRunner().EntryPoint(args);
            }
        }
    }
}
