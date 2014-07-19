using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using Xunit.Abstractions;

namespace Xunit.ConsoleClient
{
    public class AppVeyorVisitor : XmlTestExecutionVisitor
    {
        private const string FrameworkName = "xUnit";
        ConcurrentDictionary<string, int> testMethods = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        string assemblyFileName;
        readonly object consoleLock;
        readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages;
        readonly string defaultDirectory;

        public AppVeyorVisitor(object consoleLock,
                                     string defaultDirectory,
                                     XElement assemblyElement,
                                     Func<bool> cancelThunk,
                                     ConcurrentDictionary<string, ExecutionSummary> completionMessages = null)
            : base(assemblyElement, cancelThunk)
        {
            this.consoleLock = consoleLock;
            this.defaultDirectory = defaultDirectory;
            this.completionMessages = completionMessages;
        }

        protected override bool Visit(ITestAssemblyStarting assemblyStarting)
        {
            assemblyFileName = Path.GetFileName(assemblyStarting.AssemblyFileName);

            lock (consoleLock)
            {
                Console.WriteLine("Starting: {0}", assemblyFileName);
            }

            return base.Visit(assemblyStarting);
        }

        protected override bool Visit(ITestAssemblyFinished assemblyFinished)
        {
            // Base class does computation of results, so call it first.
            var result = base.Visit(assemblyFinished);

            lock (consoleLock)
            {
                Console.WriteLine("Finished: {0}", assemblyFileName);
            }

            if (completionMessages != null)
                completionMessages.TryAdd(assemblyFileName, new ExecutionSummary
                {
                    Total = assemblyFinished.TestsRun,
                    Failed = assemblyFinished.TestsFailed,
                    Skipped = assemblyFinished.TestsSkipped,
                    Time = assemblyFinished.ExecutionTime
                });

            return result;
        }

        protected override bool Visit(IErrorMessage error)
        {
            lock (consoleLock)
            {
                Console.Error.WriteLine("   {0} [FATAL]", Escape(error.ExceptionTypes[0]));
                Console.Error.WriteLine("      {0}", Escape(ExceptionUtility.CombineMessages(error)));

                WriteStackTrace(ExceptionUtility.CombineStackTraces(error));
            }

            return base.Visit(error);
        }

        protected override bool Visit(ITestStarting testStarting)
        {
            // Appveyor Logging
            string testName = testStarting.TestDisplayName;
            if (testMethods.ContainsKey(testName))
            {
                testName = testName + " " + testMethods[testName].ToString();
            }

            BuildWorkerApi.AddTest(testName, FrameworkName, assemblyFileName, "Running", null, null, null, null, null);

            return base.Visit(testStarting);
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            // Appveyor Logging
            BuildWorkerApi.UpdateTest(GetFinishedTestName(testPassed.TestDisplayName), FrameworkName, assemblyFileName, "Passed", Convert.ToInt64(testPassed.ExecutionTime * 1000),
                null, null, testPassed.Output, null);

            return base.Visit(testPassed);
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            // Appveyor Logging
            BuildWorkerApi.UpdateTest(GetFinishedTestName(testSkipped.TestDisplayName), FrameworkName, assemblyFileName, "Skipped", Convert.ToInt64(testSkipped.ExecutionTime * 1000),
                null, null, null, null);

            lock (consoleLock)
            {
                Console.Error.WriteLine("   {0} [SKIP]", Escape(testSkipped.TestDisplayName));
                Console.Error.WriteLine("      {0}", Escape(testSkipped.Reason));
            }

            return base.Visit(testSkipped);
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            // Appveyor Logging
            BuildWorkerApi.UpdateTest(GetFinishedTestName(testFailed.TestDisplayName), FrameworkName, assemblyFileName, "Failed", Convert.ToInt64(testFailed.ExecutionTime * 1000),
                ExceptionUtility.CombineMessages(testFailed), ExceptionUtility.CombineStackTraces(testFailed), testFailed.Output, null);

            lock (consoleLock)
            {
                Console.Error.WriteLine("   {0} [FAIL]", Escape(testFailed.TestDisplayName));
                Console.Error.WriteLine("      {0}", Escape(ExceptionUtility.CombineMessages(testFailed)));

                WriteStackTrace(ExceptionUtility.CombineStackTraces(testFailed));
            }

            return base.Visit(testFailed);
        }

        protected override bool Visit(IAfterTestFinished afterTestFinished)
        {
            Console.Write(".");

            return base.Visit(afterTestFinished);
        }

        private string GetFinishedTestName(string methodName)
        {
            string testName = methodName;

            int number = 0;
            if (testMethods.ContainsKey(methodName))
            {
                number = testMethods[methodName];
                testName = methodName + " " + number.ToString();
            }

            testMethods[methodName] = number + 1;

            return testName;
        }

        void WriteStackTrace(string stackTrace)
        {
            if (String.IsNullOrWhiteSpace(stackTrace))
                return;

            Console.Error.WriteLine("      Stack Trace:");
            Array.ForEach(stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None),
                          stackFrame => Console.Error.WriteLine("         {0}", StackFrameTransformer.TransformFrame(stackFrame, defaultDirectory)));
        }

        #region Build Worker API client
        public class BuildWorkerApi
        {
            private static string _apiUrl = null;

            public static void AddTest(string testName, string testFramework, string fileName,
                string outcome, long? durationMilliseconds, string errorMessage, string errorStackTrace, string stdOut, string stdErr)
            {
                if (GetApiUrl() == null)
                {
                    return;
                }

                var body = new AddUpdateTestRequest
                {
                    TestName = testName,
                    TestFramework = testFramework,
                    FileName = fileName,
                    Outcome = outcome,
                    DurationMilliseconds = durationMilliseconds,
                    ErrorMessage = errorMessage,
                    ErrorStackTrace = errorStackTrace,
                    StdOut = TrimStdOut(stdOut),
                    StdErr = TrimStdOut(stdErr)
                };

                try
                {
                    using (WebClient wc = GetClient())
                    {
                        wc.UploadData("api/tests", "POST", Json(body));
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error communicating AppVeyor Build Worker API: " + ex.Message);
                }
            }

            public static void UpdateTest(string testName, string testFramework, string fileName,
                string outcome, long? durationMilliseconds, string errorMessage, string errorStackTrace, string stdOut, string stdErr)
            {
                if (GetApiUrl() == null)
                {
                    return;
                }

                var body = new AddUpdateTestRequest
                {
                    TestName = testName,
                    TestFramework = testFramework,
                    FileName = fileName,
                    Outcome = outcome,
                    DurationMilliseconds = durationMilliseconds,
                    ErrorMessage = errorMessage,
                    ErrorStackTrace = errorStackTrace,
                    StdOut = TrimStdOut(stdOut),
                    StdErr = TrimStdOut(stdErr)
                };

                try
                {
                    using (WebClient wc = GetClient())
                    {
                        wc.UploadData("api/tests", "PUT", Json(body));
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error communicating AppVeyor Build Worker API: " + ex.Message);
                }
            }

            private static string TrimStdOut(string str)
            {
                int maxLength = 4096;

                if (str == null)
                {
                    return null;
                }

                return (str.Length > maxLength) ? str.Substring(0, maxLength) : str;
            }

            private static byte[] Json(object data)
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                var json = serializer.Serialize(data);
                return Encoding.UTF8.GetBytes(json);
            }

            private static WebClient GetClient()
            {
                WebClient wc = new WebClient();
                wc.BaseAddress = GetApiUrl();
                wc.Headers["Accept"] = "application/json";
                wc.Headers["Content-type"] = "application/json";
                return wc;
            }

            private static string GetApiUrl()
            {
                // get API URL from registry
                if (_apiUrl == null)
                {
                    _apiUrl = Environment.GetEnvironmentVariable("APPVEYOR_API_URL");

                    if (_apiUrl != null)
                    {
                        _apiUrl = _apiUrl.TrimEnd('/') + "/";
                    }
                }

                return _apiUrl;
            }

            public class WebDownload : WebClient
            {
                /// <summary>
                /// Time in milliseconds
                /// </summary>
                public int Timeout { get; set; }

                public WebDownload() : this(60000) { }

                public WebDownload(int timeout)
                {
                    this.Timeout = timeout;
                }

                protected override WebRequest GetWebRequest(Uri address)
                {
                    var request = base.GetWebRequest(address);
                    if (request != null)
                    {
                        request.Timeout = this.Timeout;
                    }
                    return request;
                }
            }

            public class AddUpdateTestRequest
            {
                public string TestName { get; set; }
                public string FileName { get; set; }
                public string TestFramework { get; set; }
                public string Outcome { get; set; }
                public long? DurationMilliseconds { get; set; }
                public string ErrorMessage { get; set; }
                public string ErrorStackTrace { get; set; }
                public string StdOut { get; set; }
                public string StdErr { get; set; }
            }
        }
        #endregion
    }
}
