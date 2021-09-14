using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class AppVeyorReporterMessageHandler : DefaultRunnerReporterWithTypesMessageHandler
    {
        const int MaxLength = 4096;

        readonly ConcurrentDictionary<string, Tuple<string, Dictionary<string, int>>> assemblyNames = new ConcurrentDictionary<string, Tuple<string, Dictionary<string, int>>>();
        readonly string baseUri;
        AppVeyorClient client;

        int assembliesInFlight;
        readonly object clientLock = new object();

        public AppVeyorReporterMessageHandler(IRunnerLogger logger, string baseUri)
            : base(logger)
        {
            this.baseUri = baseUri.TrimEnd('/');

            Execution.TestAssemblyStartingEvent += HandleTestAssemblyStarting;
            Execution.TestStartingEvent += HandleTestStarting;
            Execution.TestAssemblyFinishedEvent += HandleTestAssemblyFinished;
        }

        void HandleTestAssemblyFinished(MessageHandlerArgs<ITestAssemblyFinished> args)
        {
            lock (clientLock)
            {
                assembliesInFlight--;

                if (assembliesInFlight == 0)
                {
                    // Drain the queue
                    client.WaitOne(CancellationToken.None);
                    client = null;
                }
            }
        }

        void HandleTestAssemblyStarting(MessageHandlerArgs<ITestAssemblyStarting> args)
        {
            lock (clientLock)
            {
                assembliesInFlight++;

                // Look for the TFM attrib to disambiguate 
                var attrib = args.Message.TestAssembly.Assembly.GetCustomAttributes("System.Runtime.Versioning.TargetFrameworkAttribute").FirstOrDefault();
                var arg = attrib?.GetConstructorArguments().FirstOrDefault() as string;

                var assemblyFileName = Path.GetFileName(args.Message.TestAssembly.Assembly.AssemblyPath);
                if (arg != null)
                    assemblyFileName = $"{assemblyFileName} ({arg})";

                assemblyNames[args.Message.TestAssembly.Assembly.Name] = Tuple.Create(assemblyFileName, new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));

                if (client == null)
                    client = new AppVeyorClient(Logger, baseUri);
            }
        }

        void HandleTestStarting(MessageHandlerArgs<ITestStarting> args)
        {
            var testName = args.Message.Test.DisplayName;

            var dict = assemblyNames[args.Message.TestAssembly.Assembly.Name].Item2;
            lock (dict)
                if (dict.ContainsKey(testName))
                    testName = $"{testName} {dict[testName]}";

            AppVeyorAddTest(testName, "xUnit", assemblyNames[args.Message.TestAssembly.Assembly.Name].Item1, "Running", null, null, null, null);
        }

        protected override void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
        {
            var testPassed = args.Message;
            var dict = assemblyNames[args.Message.TestAssembly.Assembly.Name].Item2;

            AppVeyorUpdateTest(GetFinishedTestName(testPassed.Test.DisplayName, dict), "xUnit", assemblyNames[args.Message.TestAssembly.Assembly.Name].Item1, "Passed",
                               Convert.ToInt64(testPassed.ExecutionTime * 1000), null, null, testPassed.Output);

            base.HandleTestPassed(args);
        }

        protected override void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
        {
            var testSkipped = args.Message;
            var dict = assemblyNames[args.Message.TestAssembly.Assembly.Name].Item2;

            AppVeyorUpdateTest(GetFinishedTestName(testSkipped.Test.DisplayName, dict), "xUnit", assemblyNames[args.Message.TestAssembly.Assembly.Name].Item1, "Skipped",
                               Convert.ToInt64(testSkipped.ExecutionTime * 1000), null, null, null);

            base.HandleTestSkipped(args);
        }

        protected override void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
        {
            var testFailed = args.Message;

            var dict = assemblyNames[args.Message.TestAssembly.Assembly.Name].Item2;
            AppVeyorUpdateTest(GetFinishedTestName(testFailed.Test.DisplayName, dict), "xUnit", assemblyNames[args.Message.TestAssembly.Assembly.Name].Item1, "Failed",
                               Convert.ToInt64(testFailed.ExecutionTime * 1000), ExceptionUtility.CombineMessages(testFailed),
                               ExceptionUtility.CombineStackTraces(testFailed), testFailed.Output);

            base.HandleTestFailed(args);
        }

        // AppVeyor API helpers

        static string GetFinishedTestName(string methodName, Dictionary<string, int> testMethods)
        {
            lock (testMethods)
            {
                var testName = methodName;
                var number = 0;

                if (testMethods.ContainsKey(methodName))
                {
                    number = testMethods[methodName];
                    testName = $"{methodName} {number}";
                }

                testMethods[methodName] = number + 1;
                return testName;
            }
        }

        void AppVeyorAddTest(string testName, string testFramework, string fileName, string outcome, long? durationMilliseconds,
                             string errorMessage, string errorStackTrace, string stdOut)
        {
            var body = new Dictionary<string, object>
            {
                { "testName", testName },
                { "testFramework", testFramework },
                { "fileName", fileName },
                { "outcome", outcome },
                { "durationMilliseconds", durationMilliseconds },
                { "ErrorMessage", errorMessage },
                { "ErrorStackTrace", errorStackTrace },
                { "StdOut", TrimStdOut(stdOut) },
            };

            client.AddTest(body);
        }

        void AppVeyorUpdateTest(string testName, string testFramework, string fileName, string outcome, long? durationMilliseconds,
                                string errorMessage, string errorStackTrace, string stdOut)
        {
            var body = new Dictionary<string, object>
            {
                { "testName", testName },
                { "testFramework", testFramework },
                { "fileName", fileName },
                { "outcome", outcome },
                { "durationMilliseconds", durationMilliseconds },
                { "ErrorMessage", errorMessage },
                { "ErrorStackTrace", errorStackTrace },
                { "StdOut", TrimStdOut(stdOut) },
            };

            client.UpdateTest(body);
        }

        static string TrimStdOut(string str)
            => str != null && str.Length > MaxLength ? str.Substring(0, MaxLength) : str;
    }
}
