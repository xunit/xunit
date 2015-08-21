namespace Xunit.Runners
{
    /// <summary>
    /// An enumeration which indicates the type of error message (for <see cref="AssemblyRunner.OnErrorMessage"/>).
    /// </summary>
    public enum ErrorMessageType
    {
        /// <summary>An unhandled exception occurred that disrupted the execution engine</summary>
        CatastrophicError = 1,

        /// <summary>An unhandled exception happened while cleaning up from the test assembly</summary>
        TestAssemblyCleanupFailure = 10,

        /// <summary>An unhandled exception happened while cleaning up from the test collection</summary>
        TestCollectionCleanupFailure = 20,

        /// <summary>An unhandled exception happened while cleaning up from the test class</summary>
        TestClassCleanupFailure = 30,

        /// <summary>An unhandled exception happened while cleaning up from the test method</summary>
        TestMethodCleanupFailure = 40,

        /// <summary>An unhandled exception happened while cleaning up from the test case</summary>
        TestCaseCleanupFailure = 50,

        /// <summary>An unhandled exception happened while cleaning up from the test</summary>
        TestCleanupFailure = 60,
    }
}
