using System;

namespace Xunit.ConsoleClient
{
    public class ConsoleLogger
    {
        public virtual void WriteLine(string format, params object[] arg)
        {
            Console.WriteLine(format, arg);
        }
    }
}
