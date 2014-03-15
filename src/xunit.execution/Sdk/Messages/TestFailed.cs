using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestFailed"/>.
    /// </summary>
    internal class TestFailed : TestResultMessage, ITestFailed
    {
        public TestFailed(ITestCase testCase,
                          string testDisplayName,
                          decimal executionTime,
                          string output,
                          string[] exceptionTypes,
                          string[] messages,
                          string[] stackTraces,
                          int[] exceptionParentIndices)
            : base(testCase, testDisplayName, executionTime, output)
        {
            StackTraces = stackTraces;
            Messages = messages;
            ExceptionTypes = exceptionTypes;
            ExceptionParentIndices = exceptionParentIndices;
        }

#if XUNIT_CORE_DLL
        public TestFailed(ITestCase testCase,
                          string testDisplayName,
                          decimal executionTime,
                          string output,
                          Exception ex)
            : base(testCase, testDisplayName, executionTime, output)
        {
            var failureInfo = ExceptionUtility.ConvertExceptionToFailureInformation(ex);
            ExceptionTypes = failureInfo.ExceptionTypes;
            Messages = failureInfo.Messages;
            StackTraces = failureInfo.StackTraces;
            ExceptionParentIndices = failureInfo.ExceptionParentIndices;
        }
#endif

        /// <inheritdoc/>
        public string[] ExceptionTypes { get; private set; }

        /// <inheritdoc/>
        public string[] Messages { get; private set; }

        /// <inheritdoc/>
        public string[] StackTraces { get; private set; }

        /// <inheritdoc/>
        public int[] ExceptionParentIndices { get; private set; }
    }
}