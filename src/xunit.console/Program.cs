using System;

namespace Xunit.ConsoleClient
{
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
#if NET452
            using (AssemblyHelper.SubscribeResolve())
#else
            using (NetCoreAssemblyHelper.SubscribeResolve())
#endif
            {
                return new ConsoleRunner().EntryPoint(args);
            }
        }
    }
}
