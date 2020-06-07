using System;
using System.Xml.Linq;

namespace Xunit
{
    public class Transform
    {
        public string CommandLine;
        public string Description;
        public Action<XElement, string> OutputHandler;
    }
}
