using System;
using System.Collections.Generic;
using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestCollectionCleanupFailure"/>.
    /// </summary>
    public class TestCollectionCleanupFailure : TestCollectionMessage, ITestCollectionCleanupFailure
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestClassCleanupFailure"/> class.
        /// </summary>
        public TestCollectionCleanupFailure(IEnumerable<ITestCase> testCases, ITestCollection testCollection, string[] exceptionTypes, string[] messages, string[] stackTraces, int[] exceptionParentIndices)
            : base(testCases, testCollection)
        {
            StackTraces = stackTraces;
            Messages = messages;
            ExceptionTypes = exceptionTypes;
            ExceptionParentIndices = exceptionParentIndices;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestClassCleanupFailure"/> class.
        /// </summary>
        public TestCollectionCleanupFailure(IEnumerable<ITestCase> testCases, ITestCollection testCollection, Exception ex)
            : base(testCases, testCollection)
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
