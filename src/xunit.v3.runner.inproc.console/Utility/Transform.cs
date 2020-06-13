using System;
using System.Xml.Linq;

namespace Xunit.Runner.InProc.SystemConsole
{
    public class Transform
    {
        public string CommandLine;
        public string Description;
        public Action<XElement, string> OutputHandler;
    }
}
