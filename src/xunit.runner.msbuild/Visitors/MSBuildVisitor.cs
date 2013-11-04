using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Utilities;

namespace Xunit.Runner.MSBuild
{
    public class MSBuildVisitor : XmlTestExecutionVisitor
    {
        public readonly TaskLoggingHelper Log;

        public MSBuildVisitor(TaskLoggingHelper log, XElement assemblyElement, Func<bool> cancelThunk)
            : base(assemblyElement, cancelThunk)
        {
            Log = log;
        }
    }
}
