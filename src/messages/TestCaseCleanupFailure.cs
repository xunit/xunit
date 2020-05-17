using System;
using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestCaseCleanupFailure"/>.
    /// </summary>
    public class TestCaseCleanupFailure : TestCaseMessage, ITestCaseCleanupFailure
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCaseCleanupFailure"/> class.
        /// </summary>
        public TestCaseCleanupFailure(ITestCase testCase, string[] exceptionTypes, string[] messages, string[] stackTraces, int[] exceptionParentIndices)
            : base(testCase)
        {
            StackTraces = stackTraces;
            Messages = messages;
            ExceptionTypes = exceptionTypes;
            ExceptionParentIndices = exceptionParentIndices;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCaseCleanupFailure"/> class.
        /// </summary>
        public TestCaseCleanupFailure(ITestCase testCase, Exception ex)
            : base(testCase)
        {
            var failureInfo = ExceptionUtility.ConvertExceptionToFailureInformation(ex);
            ExceptionTypes = failureInfo.ExceptionTypes;
            Messages = failureInfo.Messages;
            StackTraces = failureInfo.StackTraces;
            ExceptionParentIndices = failureInfo.ExceptionParentIndices;
        }

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
