using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Xunit.Runner.MSBuild
{
    public class TeamCityLogger : IRunnerLogger
    {
        readonly TaskLoggingHelper log;

        public TeamCityLogger(TaskLoggingHelper log)
        {
            this.log = log;
        }

        public void AssemblyFinished(string assemblyFilename, int total, int failed, int skipped, double time)
        {
            log.LogMessage(MessageImportance.High, "##teamcity[testSuiteFinished name='{0}']", Escape(Path.GetFileName(assemblyFilename)));
        }

        public void AssemblyStart(string assemblyFilename, string configFilename, string xUnitVersion)
        {
            log.LogMessage(MessageImportance.High, "##teamcity[testSuiteStarted name='{0}']", Escape(Path.GetFileName(assemblyFilename)));
        }

        public bool ClassFailed(string className, string exceptionType, string message, string stackTrace)
        {
            log.LogMessage(MessageImportance.High, "##teamcity[buildStatus status='FAILURE' text='Class failed: {0}: {1}|r|n{2}']",
                           Escape(className),
                           Escape(message),
                           Escape(stackTrace));
            return true;
        }

        public void ExceptionThrown(string assemblyFilename, Exception exception)
        {
            log.LogError(exception.Message);
            log.LogError("While running: {0}", assemblyFilename);
        }

        public void TestFailed(string name, string type, string method, double duration, string output, string exceptionType, string message, string stackTrace)
        {
            log.LogMessage(MessageImportance.High, "##teamcity[testFailed name='{0}' details='{1}|r|n{2}']",
                           Escape(name),
                           Escape(message),
                           Escape(stackTrace));
            WriteOutput(name, output);
            WriteFinished(name, duration);
        }

        public bool TestFinished(string name, string type, string method)
        {
            return true;
        }

        public void TestPassed(string name, string type, string method, double duration, string output)
        {
            WriteOutput(name, output);
            WriteFinished(name, duration);
        }

        public void TestSkipped(string name, string type, string method, string reason)
        {
            log.LogMessage(MessageImportance.High, "##teamcity[testIgnored name='{0}' message='{1}']",
                           Escape(name),
                           Escape(reason));

            WriteFinished(name, 0);
        }

        public bool TestStart(string name, string type, string method)
        {
            log.LogMessage(MessageImportance.High, "##teamcity[testStarted name='{0}']", Escape(name));
            return true;
        }

        static string Escape(string value)
        {
            if (value == null)
                return String.Empty;

            return value.Replace("|", "||")
                        .Replace("'", "|'")
                        .Replace("\r", "|r")
                        .Replace("\n", "|n")
                        .Replace("]", "|]");
        }

        void WriteFinished(string name, double duration)
        {
            log.LogMessage(MessageImportance.High, "##teamcity[testFinished name='{0}' duration='{1}']",
                                       Escape(name),
                                       (int)(duration * 1000D));
        }

        void WriteOutput(string name, string output)
        {
            if (output != null)
                log.LogMessage(MessageImportance.High, "##teamcity[testStdOut name='{0}' out='{1}']", Escape(name), Escape(output));
        }
    }
}