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
    public class VstsReporterMessageHandler : DefaultRunnerReporterWithTypesMessageHandler
    {
        const int MaxLength = 4096;

        readonly ConcurrentDictionary<string, string> assemblyNames = new ConcurrentDictionary<string, string>();
        readonly string baseUri;
        VstsClient client;

        int assembliesInFlight; 
        readonly object clientLock = new object();
        readonly string accessToken;
        readonly int buildId;

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
                var arg = attrib?.GetConstructorArguments().FirstOrDefault() as string;
                
                var assemblyFileName = Path.GetFileName(args.Message.TestAssembly.Assembly.AssemblyPath);
                if (arg != null)
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
                        args.Message.TestCase.UniqueID);
        }

        protected override void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
        {
            var testPassed = args.Message;

            VstsUpdateTest(args.Message.TestCase.UniqueID, "Passed",
                               Convert.ToInt64(testPassed.ExecutionTime * 1000), null, null, testPassed.Output);

            base.HandleTestPassed(args);
        }

        protected override void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
        {
            var testSkipped = args.Message;


            VstsUpdateTest(args.Message.TestCase.UniqueID, "NotExecuted",
                               Convert.ToInt64(testSkipped.ExecutionTime * 1000), null, null, null);

            base.HandleTestSkipped(args);
        }

        protected override void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
        {
            var testFailed = args.Message;

            VstsUpdateTest(args.Message.TestCase.UniqueID, "Failed",
                               Convert.ToInt64(testFailed.ExecutionTime * 1000), ExceptionUtility.CombineMessages(testFailed),
                               ExceptionUtility.CombineStackTraces(testFailed), testFailed.Output);

            base.HandleTestFailed(args);
        }
        

        void VstsAddTest(string testName, string displayName, string fileName, string uniqueId)
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

        void VstsUpdateTest(string uniqueId, string outcome, long? durationMilliseconds,
                                string errorMessage, string errorStackTrace, string stdOut)
        {
            var body = new Dictionary<string, object>
            {
                { "outcome", outcome },
                { "durationInMs", durationMilliseconds },
                { "errorMessage", $"{errorMessage}\n{errorStackTrace}\n{TrimStdOut(stdOut)}" },
                { "state", "Completed" },
                { "completedDate", DateTime.UtcNow }
            };

            client.UpdateTest(body, uniqueId);
        }

        static string TrimStdOut(string str)
            => str != null && str.Length > MaxLength ? str.Substring(0, MaxLength) : str;
    }
}
