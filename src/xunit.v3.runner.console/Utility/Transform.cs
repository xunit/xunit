using System;
using System.Xml.Linq;

namespace Xunit.Runner.SystemConsole
{
    public class Transform
    {
        public string CommandLine;
        public string Description;
        public Action<XElement, string> OutputHandler;
    }
}
