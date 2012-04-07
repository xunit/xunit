using Microsoft.Build.Utilities;

namespace Xunit.Runner.MSBuild
{
    public class VerboseLogger : StandardLogger
    {
        public VerboseLogger(TaskLoggingHelper log) : base(log) { }

        public override void TestPassed(string name, string type, string method, double duration, string output)
        {
            log.LogMessage("    PASS:  {0}", name);

            WriteOutput(output);
        }

        public override bool TestStart(string name, string type, string method)
        {
            log.LogMessage("    START: {0}", name);
            return true;
        }
    }
}