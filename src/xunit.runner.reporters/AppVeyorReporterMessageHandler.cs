#if !DNXCORE50    // TODO: Add conditional code for DNX to use JSON.NET instead of JSS

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class AppVeyorReporterMessageHandler : DefaultRunnerReporterMessageHandler
    {
        string assemblyFileName;
        readonly ConcurrentDictionary<string, int> testMethods = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public AppVeyorReporterMessageHandler(IRunnerLogger logger) : base(logger) { }

        protected override bool Visit(ITestAssemblyStarting assemblyStarting)
        {
            assemblyFileName = Path.GetFileName(assemblyStarting.TestAssembly.Assembly.AssemblyPath);

            return base.Visit(assemblyStarting);
        }

        protected override bool Visit(ITestStarting testStarting)
        {
            var testName = testStarting.Test.DisplayName;
            if (testMethods.ContainsKey(testName))
                testName = string.Format("{0} {1}", testName, testMethods[testName]);

            AppVeyorAddTest(testName, "xUnit", assemblyFileName, "Running", null, null, null, null, null);

            return base.Visit(testStarting);
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            AppVeyorUpdateTest(GetFinishedTestName(testPassed.Test.DisplayName), "xUnit", assemblyFileName, "Passed",
                               Convert.ToInt64(testPassed.ExecutionTime * 1000), null, null, testPassed.Output, null);

            return base.Visit(testPassed);
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            AppVeyorUpdateTest(GetFinishedTestName(testSkipped.Test.DisplayName), "xUnit", assemblyFileName, "Skipped",
                               Convert.ToInt64(testSkipped.ExecutionTime * 1000), null, null, null, null);

            return base.Visit(testSkipped);
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            AppVeyorUpdateTest(GetFinishedTestName(testFailed.Test.DisplayName), "xUnit", assemblyFileName, "Failed",
                               Convert.ToInt64(testFailed.ExecutionTime * 1000), ExceptionUtility.CombineMessages(testFailed),
                               ExceptionUtility.CombineStackTraces(testFailed), testFailed.Output, null);

            return base.Visit(testFailed);
        }

        // AppVeyor API helpers

        const int MaxLength = 4096;

        static Lazy<string> apiUrl = new Lazy<string>(GetApiUri);

        static string ApiUri
        {
            get { return apiUrl.Value; }
        }

        string GetFinishedTestName(string methodName)
        {
            var testName = methodName;
            var number = 0;

            if (testMethods.ContainsKey(methodName))
            {
                number = testMethods[methodName];
                testName = string.Format("{0} {1}", methodName, number);
            }

            testMethods[methodName] = number + 1;
            return testName;
        }

        static void AppVeyorAddTest(string testName, string testFramework, string fileName, string outcome, long? durationMilliseconds,
                                    string errorMessage, string errorStackTrace, string stdOut, string stdErr)
        {
            if (ApiUri == null)
                return;

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
                using (var wc = GetClient())
                    wc.UploadData("api/tests", "POST", Json(body));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error communicating AppVeyor Build Worker API: " + ex.Message);
            }
        }

        static void AppVeyorUpdateTest(string testName, string testFramework, string fileName, string outcome, long? durationMilliseconds,
                                       string errorMessage, string errorStackTrace, string stdOut, string stdErr)
        {
            if (ApiUri == null)
                return;

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
                using (var wc = GetClient())
                    wc.UploadData("api/tests", "PUT", Json(body));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error communicating AppVeyor Build Worker API: " + ex.Message);
            }
        }

        static string TrimStdOut(string str)
        {
            return str != null && str.Length > MaxLength ? str.Substring(0, MaxLength) : str;
        }

        static byte[] Json(object data)
        {
            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(data);
            return Encoding.UTF8.GetBytes(json);
        }

        static WebClient GetClient()
        {
            var wc = new WebClient() { BaseAddress = ApiUri };
            wc.Headers["Accept"] = "application/json";
            wc.Headers["Content-type"] = "application/json";
            return wc;
        }

        static string GetApiUri()
        {
            var apiUrl = Environment.GetEnvironmentVariable("APPVEYOR_API_URL");

            if (apiUrl != null)
                apiUrl = apiUrl.TrimEnd('/') + "/";

            return apiUrl;
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
}

#endif