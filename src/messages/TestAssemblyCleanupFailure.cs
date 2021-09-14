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
    /// Default implementation of <see cref="TestAssemblyCleanupFailure"/>.
    /// </summary>
    public class TestAssemblyCleanupFailure : TestAssemblyMessage, ITestAssemblyCleanupFailure
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssemblyCleanupFailure"/> class.
        /// </summary>
        public TestAssemblyCleanupFailure(IEnumerable<ITestCase> testCases, ITestAssembly testAssembly, string[] exceptionTypes, string[] messages, string[] stackTraces, int[] exceptionParentIndices)
            : base(testCases, testAssembly)
        {
            StackTraces = stackTraces;
            Messages = messages;
            ExceptionTypes = exceptionTypes;
            ExceptionParentIndices = exceptionParentIndices;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssemblyCleanupFailure"/> class.
        /// </summary>
        public TestAssemblyCleanupFailure(IEnumerable<ITestCase> testCases, ITestAssembly testAssembly, Exception ex)
            : base(testCases, testAssembly)
        {
            var failureInfo = ExceptionUtility.ConvertExceptionToFailureInformation(ex);
            ExceptionTypes = failureInfo.ExceptionTypes;
            Messages = failureInfo.Messages;
            StackTraces = failureInfo.StackTraces;
            ExceptionParentIndices = failureInfo.ExceptionParentIndices;
        }

        /// <inheritdoc/>
        public int[] ExceptionParentIndices { get; private set; }

        /// <inheritdoc/>
        public string[] ExceptionTypes { get; private set; }

        /// <inheritdoc/>
        public string[] Messages { get; private set; }

        /// <inheritdoc/>
        public string[] StackTraces { get; private set; }
    }
}
