using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class AppVeyorReporterMessageHandler : DefaultRunnerReporterWithTypesMessageHandler
    {
        const int MaxLength = 4096;

        string assemblyFileName;
        readonly string baseUri;
        readonly Dictionary<string, int> testMethods = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        AppVeyorClient client;

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
            client.WaitOne(CancellationToken.None);
        }

        void HandleTestAssemblyStarting(MessageHandlerArgs<ITestAssemblyStarting> args)
        {
            assemblyFileName = Path.GetFileName(args.Message.TestAssembly.Assembly.AssemblyPath);
            client = new AppVeyorClient(Logger, baseUri);
        }

        void HandleTestStarting(MessageHandlerArgs<ITestStarting> args)
        {
            var testName = args.Message.Test.DisplayName;

            lock (testMethods)
                if (testMethods.ContainsKey(testName))
                    testName = $"{testName} {testMethods[testName]}";

            AppVeyorAddTest(testName, "xUnit", assemblyFileName, "Running", null, null, null, null);
        }

        protected override void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
        {
            var testPassed = args.Message;

            AppVeyorUpdateTest(GetFinishedTestName(testPassed.Test.DisplayName), "xUnit", assemblyFileName, "Passed",
                               Convert.ToInt64(testPassed.ExecutionTime * 1000), null, null, testPassed.Output);

            base.HandleTestPassed(args);
        }

        protected override void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
        {
            var testSkipped = args.Message;

            AppVeyorUpdateTest(GetFinishedTestName(testSkipped.Test.DisplayName), "xUnit", assemblyFileName, "Skipped",
                               Convert.ToInt64(testSkipped.ExecutionTime * 1000), null, null, null);

            base.HandleTestSkipped(args);
        }

        protected override void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
        {
            var testFailed = args.Message;

            AppVeyorUpdateTest(GetFinishedTestName(testFailed.Test.DisplayName), "xUnit", assemblyFileName, "Failed",
                               Convert.ToInt64(testFailed.ExecutionTime * 1000), ExceptionUtility.CombineMessages(testFailed),
                               ExceptionUtility.CombineStackTraces(testFailed), testFailed.Output);

            base.HandleTestFailed(args);
        }

        // AppVeyor API helpers

        string GetFinishedTestName(string methodName)
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
