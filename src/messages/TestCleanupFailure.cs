using System;
using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
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
        public TestCleanupFailure(ITest test, string[] exceptionTypes, string[] messages, string[] stackTraces, int[] exceptionParentIndices)
            : base(test)
        {
            StackTraces = stackTraces;
            Messages = messages;
            ExceptionTypes = exceptionTypes;
            ExceptionParentIndices = exceptionParentIndices;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCleanupFailure"/> class.
        /// </summary>
        public TestCleanupFailure(ITest test, Exception ex)
            : base(test)
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
