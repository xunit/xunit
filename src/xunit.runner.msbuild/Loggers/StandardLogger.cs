using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Xunit.Runner.MSBuild
{
    public class StandardLogger : IRunnerLogger
    {
        protected readonly Func<bool> cancelled;
        protected readonly TaskLoggingHelper log;
        public int Total = 0;
        public int Failed = 0;
        public int Skipped = 0;
        public double Time = 0.0;

        public StandardLogger(TaskLoggingHelper log, Func<bool> cancelled)
        {
            this.cancelled = cancelled;
            this.log = log;
        }

        public void AssemblyFinished(string assemblyFilename, int total, int failed, int skipped, double time)
        {
            log.LogMessage(MessageImportance.High,
                           "  Tests: {0}, Failures: {1}, Skipped: {2}, Time: {3} seconds",
                           total,
                           failed,
                           skipped,
                           time.ToString("0.000"));

            Total += total;
            Failed += failed;
            Skipped += skipped;
            Time += time;
        }

        public void AssemblyStart(string assemblyFilename, string configFilename, string xUnitVersion)
        {
        }

        public bool ClassFailed(string className, string exceptionType, string message, string stackTrace)
        {
            log.LogError("[CLASS] {0}: {1}", className, Escape(message));
            log.LogError(Escape(stackTrace));
            return !cancelled();
        }

        public void ExceptionThrown(string assemblyFilename, Exception exception)
        {
            log.LogError(exception.Message);
            log.LogError("While running: {0}", assemblyFilename);
        }

        public void TestFailed(string name, string type, string method, double duration, string output, string exceptionType, string message, string stackTrace)
        {
            log.LogError("{0}: {1}", name, Escape(message));
            log.LogError(Escape(stackTrace));
            WriteOutput(output);
        }

        public bool TestFinished(string name, string type, string method)
        {
            return !cancelled();
        }

        public virtual void TestPassed(string name, string type, string method, double duration, string output)
        {
            log.LogMessage("    {0}", name);
            WriteOutput(output);
        }

        public void TestSkipped(string name, string type, string method, string reason)
        {
            log.LogWarning("{0}: {1}", name, Escape(reason));
        }

        public virtual bool TestStart(string name, string type, string method)
        {
            return !cancelled();
        }

        static string Escape(string value)
        {
            if (value == null)
                return String.Empty;

            return value.Replace(Environment.NewLine, "\n");
        }

        protected void WriteOutput(string output)
        {
            if (output != null)
            {
                log.LogMessage("    Captured output:");
                foreach (string line in output.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    log.LogMessage("      {0}", line);
            }
        }
    }
}