using System;
using System.Collections.Generic;
using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestMethodCleanupFailure"/>.
    /// </summary>
    public class TestMethodCleanupFailure : TestMethodMessage, ITestMethodCleanupFailure
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage"/> class.
        /// </summary>
        public TestMethodCleanupFailure(IEnumerable<ITestCase> testCases, ITestMethod testMethod, string[] exceptionTypes, string[] messages, string[] stackTraces, int[] exceptionParentIndices)
            : base(testCases, testMethod)
        {
            StackTraces = stackTraces;
            Messages = messages;
            ExceptionTypes = exceptionTypes;
            ExceptionParentIndices = exceptionParentIndices;
        }

#if XUNIT_CORE_DLL
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage"/> class.
        /// </summary>
        public TestMethodCleanupFailure(IEnumerable<ITestCase> testCases, ITestMethod testMethod, Exception ex)
            : base(testCases, testMethod)
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