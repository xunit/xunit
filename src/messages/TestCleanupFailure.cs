using System;
using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestCleanupFailure"/>.
    /// </summary>
    public class TestCleanupFailure : TestMessage, ITestCleanupFailure
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCleanupFailure"/> class.
        /// </summary>
        public TestCleanupFailure(ITestCase testCase, string displayName, string[] exceptionTypes, string[] messages, string[] stackTraces, int[] exceptionParentIndices)
            : base(testCase, displayName)
        {
            StackTraces = stackTraces;
            Messages = messages;
            ExceptionTypes = exceptionTypes;
            ExceptionParentIndices = exceptionParentIndices;
        }

#if XUNIT_CORE_DLL
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCleanupFailure"/> class.
        /// </summary>
        public TestCleanupFailure(ITestCase testCase, string displayName, Exception ex)
            : base(testCase, displayName)
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