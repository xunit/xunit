using System.Collections.Generic;

namespace Xunit
{
    /// <summary>
    /// Called when a diagnostic message was issued.
    /// </summary>
    /// <param name="message">The diagnostic message</param>
    public delegate void DiagnosticMessageHandler(string message);

    /// <summary>
    /// Called when test discovery has been completed.
    /// </summary>
    /// <param name="testCasesDiscovered">The number of test cases discovered</param>
    /// <param name="testCasesToRun">The number of test cases to be run (after filtering was applied)</param>
    public delegate void DiscoveryCompleteHandler(int testCasesDiscovered, int testCasesToRun);

    /// <summary>
    /// Called when an error (which isn't a test failure) was encountered.
    /// </summary>
    /// <param name="messageType">When the error condition was encountered</param>
    /// <param name="exceptionType">The type of the exception thrown</param>
    /// <param name="message">The exception message</param>
    /// <param name="stackTrace">The stack trace</param>
    public delegate void ErrorMessageHandler(ErrorMessageType messageType, string exceptionType, string message, string stackTrace);

    /// <summary>
    /// Called when test execution has been completed.
    /// </summary>
    /// <param name="totalTests">The total number of tests run</param>
    /// <param name="testsFailed">The number of tests which failed</param>
    /// <param name="testsSkipped">The number of tests which were skipped</param>
    /// <param name="executionTime">The execution time taken to run all the tests</param>
    public delegate void ExecutionCompleteHandler(int totalTests, int testsFailed, int testsSkipped, decimal executionTime);

    /// <summary>
    /// Called when a failing test has finished executing.
    /// </summary>
    /// <param name="typeName">The full name of the type that contains the test</param>
    /// <param name="methodName">The name of the method that contains the test</param>
    /// <param name="traits">The traits associated with the test</param>
    /// <param name="testDisplayName">The display name of the test</param>
    /// <param name="testCollectionDisplayName">The display name of the test collect the test belongs to</param>
    /// <param name="executionTime">The execution time, in seconds</param>
    /// <param name="output">The output from the test, if any</param>
    /// <param name="exceptionType">The type of the exception thrown</param>
    /// <param name="message">The exception message</param>
    /// <param name="stackTrace">The stack trace</param>
    public delegate void TestFailedHandler(string typeName, string methodName, Dictionary<string, List<string>> traits, string testDisplayName, string testCollectionDisplayName, decimal executionTime, string output, string exceptionType, string message, string stackTrace);

    /// <summary>
    /// Called when a test has finished executing (regardless of outcome).
    /// </summary>
    /// <param name="typeName">The full name of the type that contains the test</param>
    /// <param name="methodName">The name of the method that contains the test</param>
    /// <param name="traits">The traits associated with the test</param>
    /// <param name="testDisplayName">The display name of the test</param>
    /// <param name="testCollectionDisplayName">The display name of the test collect the test belongs to</param>
    /// <param name="executionTime">The execution time, in seconds</param>
    /// <param name="output">The output from the test, if any</param>
    public delegate void TestFinishedHandler(string typeName, string methodName, Dictionary<string, List<string>> traits, string testDisplayName, string testCollectionDisplayName, decimal executionTime, string output);

    /// <summary>
    /// Called when a line of output was written by a test. This is called when the actual output happened.
    /// The output is also collected and passed back during the test completion message(s). Note that live
    /// output is only supported by xUnit.net v2 unit tests.
    /// </summary>
    /// <param name="typeName">The full name of the type that contains the test</param>
    /// <param name="methodName">The name of the method that contains the test</param>
    /// <param name="traits">The traits associated with the test</param>
    /// <param name="testDisplayName">The display name of the test</param>
    /// <param name="testCollectionDisplayName">The display name of the test collect the test belongs to</param>
    /// <param name="output">The output from the test</param>
    public delegate void TestOutputHandler(string typeName, string methodName, Dictionary<string, List<string>> traits, string testDisplayName, string testCollectionDisplayName, string output);

    /// <summary>
    /// Called when a passing test has finished executing.
    /// </summary>
    /// <param name="typeName">The full name of the type that contains the test</param>
    /// <param name="methodName">The name of the method that contains the test</param>
    /// <param name="traits">The traits associated with the test</param>
    /// <param name="testDisplayName">The display name of the test</param>
    /// <param name="testCollectionDisplayName">The display name of the test collect the test belongs to</param>
    /// <param name="executionTime">The execution time, in seconds</param>
    /// <param name="output">The output from the test, if any</param>
    public delegate void TestPassedHandler(string typeName, string methodName, Dictionary<string, List<string>> traits, string testDisplayName, string testCollectionDisplayName, decimal executionTime, string output);

    /// <summary>
    /// Called when a test was skipped.
    /// </summary>
    /// <param name="typeName">The full name of the type that contains the test</param>
    /// <param name="methodName">The name of the method that contains the test</param>
    /// <param name="traits">The traits associated with the test</param>
    /// <param name="testDisplayName">The display name of the test</param>
    /// <param name="testCollectionDisplayName">The display name of the test collect the test belongs to</param>
    /// <param name="skipReason">The reason the test was skipped</param>
    public delegate void TestSkippedHandler(string typeName, string methodName, Dictionary<string, List<string>> traits, string testDisplayName, string testCollectionDisplayName, string skipReason);

    /// <summary>
    /// Called when a test starts executing.
    /// </summary>
    /// <param name="typeName">The full name of the type that contains the test</param>
    /// <param name="methodName">The name of the method that contains the test</param>
    /// <param name="traits">The traits associated with the test</param>
    /// <param name="testDisplayName">The display name of the test</param>
    /// <param name="testCollectionDisplayName">The display name of the test collect the test belongs to</param>
    public delegate void TestStartingHandler(string typeName, string methodName, Dictionary<string, List<string>> traits, string testDisplayName, string testCollectionDisplayName);
}
