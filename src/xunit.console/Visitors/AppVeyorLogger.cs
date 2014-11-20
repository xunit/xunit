using System;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace Xunit.ConsoleClient
{
    public class AppVeyorLogger
    {
        const int MaxLength = 4096;

        static string apiUrl;

        public static void AddTest(string testName, string testFramework, string fileName, string outcome, long? durationMilliseconds,
                                   string errorMessage, string errorStackTrace, string stdOut, string stdErr)
        {
            if (GetApiUrl() == null)
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

        public static void UpdateTest(string testName, string testFramework, string fileName, string outcome, long? durationMilliseconds,
                                      string errorMessage, string errorStackTrace, string stdOut, string stdErr)
        {
            if (GetApiUrl() == null)
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
            var wc = new WebClient() { BaseAddress = GetApiUrl() };
            wc.Headers["Accept"] = "application/json";
            wc.Headers["Content-type"] = "application/json";
            return wc;
        }

        static string GetApiUrl()
        {
            if (apiUrl == null)
            {
                apiUrl = Environment.GetEnvironmentVariable("APPVEYOR_API_URL");

                if (apiUrl != null)
                    apiUrl = apiUrl.TrimEnd('/') + "/";
            }

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
