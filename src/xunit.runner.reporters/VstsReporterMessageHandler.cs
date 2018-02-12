using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class VstsReporterMessageHandler : DefaultRunnerReporterWithTypesMessageHandler
    {
        const int MaxLength = 4096;

        readonly string accessToken;
        int assembliesInFlight;
        readonly ConcurrentDictionary<string, string> assemblyNames = new ConcurrentDictionary<string, string>();
        readonly string baseUri;
        readonly int buildId;
        VstsClient client;
        readonly object clientLock = new object();

        public VstsReporterMessageHandler(IRunnerLogger logger, string baseUri, string accessToken, int buildId)
            : base(logger)
        {
            this.baseUri = baseUri;
            this.accessToken = accessToken;
            this.buildId = buildId;

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
                    client.Dispose();
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
                var assemblyFileName = Path.GetFileName(args.Message.TestAssembly.Assembly.AssemblyPath);
                if (attrib?.GetConstructorArguments().FirstOrDefault() is string arg)
                    assemblyFileName = $"{assemblyFileName} ({arg})";

                assemblyNames[args.Message.TestAssembly.Assembly.Name] = assemblyFileName;

                if (client == null)
                    client = new VstsClient(Logger, baseUri, accessToken, buildId);
            }
        }

        void HandleTestStarting(MessageHandlerArgs<ITestStarting> args)
        {
            var assemblyName = assemblyNames[args.Message.TestAssembly.Assembly.Name];

            VstsAddTest($"{args.Message.TestClass.Class.Name}.{args.Message.TestMethod.Method.Name}",
                        args.Message.Test.DisplayName,
                        assemblyName,
                        args.Message.Test);
        }

        protected override void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
        {
            var testPassed = args.Message;

            VstsUpdateTest(args.Message.Test, "Passed",
                           Convert.ToInt64(testPassed.ExecutionTime * 1000), null, null, testPassed.Output);

            base.HandleTestPassed(args);
        }

        protected override void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
        {
            var testSkipped = args.Message;

            VstsUpdateTest(args.Message.Test, "NotExecuted",
                           Convert.ToInt64(testSkipped.ExecutionTime * 1000), null, null, null);

            base.HandleTestSkipped(args);
        }

        protected override void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
        {
            var testFailed = args.Message;

            VstsUpdateTest(args.Message.Test, "Failed",
                           Convert.ToInt64(testFailed.ExecutionTime * 1000), ExceptionUtility.CombineMessages(testFailed),
                           ExceptionUtility.CombineStackTraces(testFailed), testFailed.Output);

            base.HandleTestFailed(args);
        }

        void VstsAddTest(string testName, string displayName, string fileName, ITest uniqueId)
        {
            var body = new Dictionary<string, object>
            {
                { "testCaseTitle", displayName },
                { "automatedTestName", testName },
                { "automatedTestType", "UnitTest" },
                { "automatedTestTypeId", "13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" }, // This is used in the sample response and also appears in web searches
                { "automatedTestId", uniqueId },
                { "automatedTestStorage", fileName },
                { "state", "InProgress" },
                { "startedDate", DateTime.UtcNow }
            };

            client.AddTest(body, uniqueId);
        }

        void VstsUpdateTest(ITest uniqueId, string outcome, long? durationMilliseconds, string errorMessage, string errorStackTrace, string stdOut)
        {
            var body = new Dictionary<string, object>
            {
                { "outcome", outcome },
                { "durationInMs", durationMilliseconds },
                { "state", "Completed" }
            };

            var msg = $"{errorMessage}\n{errorStackTrace}\n{TrimStdOut(stdOut)}".Trim();
            if (!string.IsNullOrWhiteSpace(msg))
                body.Add("errorMessage", msg);

            client.UpdateTest(body, uniqueId);
        }

        static string TrimStdOut(string str)
            => str?.Length > MaxLength ? str.Substring(0, MaxLength) : str;
    }
}
