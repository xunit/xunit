using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Xunit.ConsoleClient
{
    public class Transform
    {
        public string CommandLine;
        public string Description;
        public Action<XElement, string> OutputHandler;
    }
}
