using System;
using System.Collections.Generic;
using Xunit.ConsoleClient;

public class SpyConsoleLogger : ConsoleLogger
{
    public List<string> Messages = new List<string>();

    public override void WriteLine(string format, params object[] arg)
    {
        Messages.Add(string.Format(format, arg));
    }
}
