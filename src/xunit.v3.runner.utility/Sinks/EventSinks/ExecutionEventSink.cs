using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Class that maps test framework execution messages to events.
    /// </summary>
    public class ExecutionEventSink : LongLivedMarshalByRefObject, IMessageSinkWithTypes
    {
        /// <summary>
        /// Occurs when a <see cref="IAfterTestFinished"/> message is received.
        /// </summary>
        public event MessageHandler<IAfterTestFinished> AfterTestFinishedEvent;

        /// <summary>
        /// Occurs when a <see cref="IAfterTestStarting"/> message is received.
        /// </summary>
        public event MessageHandler<IAfterTestStarting> AfterTestStartingEvent;

        /// <summary>
        /// Occurs when a <see cref="IBeforeTestFinished"/> message is received.
        /// </summary>
        public event MessageHandler<IBeforeTestFinished> BeforeTestFinishedEvent;

        /// <summary>
        /// Occurs when a <see cref="IBeforeTestStarting"/> message is received.
        /// </summary>
        public event MessageHandler<IBeforeTestStarting> BeforeTestStartingEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestAssemblyCleanupFailure"/> message is received.
        /// </summary>
        public event MessageHandler<ITestAssemblyCleanupFailure> TestAssemblyCleanupFailureEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestAssemblyFinished"/> message is received.
        /// </summary>
        public event MessageHandler<ITestAssemblyFinished> TestAssemblyFinishedEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestAssemblyStarting"/> message is received.
        /// </summary>
        public event MessageHandler<ITestAssemblyStarting> TestAssemblyStartingEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestCaseCleanupFailure"/> message is received.
        /// </summary>
        public event MessageHandler<ITestCaseCleanupFailure> TestCaseCleanupFailureEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestCaseFinished"/> message is received.
        /// </summary>
        public event MessageHandler<ITestCaseFinished> TestCaseFinishedEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestCaseStarting"/> message is received.
        /// </summary>
        public event MessageHandler<ITestCaseStarting> TestCaseStartingEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestClassCleanupFailure"/> message is received.
        /// </summary>
        public event MessageHandler<ITestClassCleanupFailure> TestClassCleanupFailureEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestClassConstructionFinished"/> message is received.
        /// </summary>
        public event MessageHandler<ITestClassConstructionFinished> TestClassConstructionFinishedEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestClassConstructionStarting"/> message is received.
        /// </summary>
        public event MessageHandler<ITestClassConstructionStarting> TestClassConstructionStartingEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestClassDisposeFinished"/> message is received.
        /// </summary>
        public event MessageHandler<ITestClassDisposeFinished> TestClassDisposeFinishedEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestClassDisposeStarting"/> message is received.
        /// </summary>
        public event MessageHandler<ITestClassDisposeStarting> TestClassDisposeStartingEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestClassFinished"/> message is received.
        /// </summary>
        public event MessageHandler<ITestClassFinished> TestClassFinishedEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestClassStarting"/> message is received.
        /// </summary>
        public event MessageHandler<ITestClassStarting> TestClassStartingEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestCleanupFailure"/> message is received.
        /// </summary>
        public event MessageHandler<ITestCleanupFailure> TestCleanupFailureEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestCollectionCleanupFailure"/> message is received.
        /// </summary>
        public event MessageHandler<ITestCollectionCleanupFailure> TestCollectionCleanupFailureEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestCollectionFinished"/> message is received.
        /// </summary>
        public event MessageHandler<ITestCollectionFinished> TestCollectionFinishedEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestCollectionStarting"/> message is received.
        /// </summary>
        public event MessageHandler<ITestCollectionStarting> TestCollectionStartingEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestFailed"/> message is received.
        /// </summary>
        public event MessageHandler<ITestFailed> TestFailedEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestFinished"/> message is received.
        /// </summary>
        public event MessageHandler<ITestFinished> TestFinishedEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestMethodCleanupFailure"/> message is received.
        /// </summary>
        public event MessageHandler<ITestMethodCleanupFailure> TestMethodCleanupFailureEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestMethodFinished"/> message is received.
        /// </summary>
        public event MessageHandler<ITestMethodFinished> TestMethodFinishedEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestMethodStarting"/> message is received.
        /// </summary>
        public event MessageHandler<ITestMethodStarting> TestMethodStartingEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestOutput"/> message is received.
        /// </summary>
        public event MessageHandler<ITestOutput> TestOutputEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestPassed"/> message is received.
        /// </summary>
        public event MessageHandler<ITestPassed> TestPassedEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestSkipped"/> message is received.
        /// </summary>
        public event MessageHandler<ITestSkipped> TestSkippedEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestStarting"/> message is received.
        /// </summary>
        public event MessageHandler<ITestStarting> TestStartingEvent;

        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
        public bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> messageTypes)
            => message.Dispatch(messageTypes, AfterTestFinishedEvent)
            && message.Dispatch(messageTypes, AfterTestStartingEvent)
            && message.Dispatch(messageTypes, BeforeTestFinishedEvent)
            && message.Dispatch(messageTypes, BeforeTestStartingEvent)
            && message.Dispatch(messageTypes, TestAssemblyCleanupFailureEvent)
            && message.Dispatch(messageTypes, TestAssemblyFinishedEvent)
            && message.Dispatch(messageTypes, TestAssemblyStartingEvent)
            && message.Dispatch(messageTypes, TestCaseCleanupFailureEvent)
            && message.Dispatch(messageTypes, TestCaseFinishedEvent)
            && message.Dispatch(messageTypes, TestCaseStartingEvent)
            && message.Dispatch(messageTypes, TestClassCleanupFailureEvent)
            && message.Dispatch(messageTypes, TestClassConstructionFinishedEvent)
            && message.Dispatch(messageTypes, TestClassConstructionStartingEvent)
            && message.Dispatch(messageTypes, TestClassDisposeFinishedEvent)
            && message.Dispatch(messageTypes, TestClassDisposeStartingEvent)
            && message.Dispatch(messageTypes, TestClassFinishedEvent)
            && message.Dispatch(messageTypes, TestClassStartingEvent)
            && message.Dispatch(messageTypes, TestCleanupFailureEvent)
            && message.Dispatch(messageTypes, TestCollectionCleanupFailureEvent)
            && message.Dispatch(messageTypes, TestCollectionFinishedEvent)
            && message.Dispatch(messageTypes, TestCollectionStartingEvent)
            && message.Dispatch(messageTypes, TestFailedEvent)
            && message.Dispatch(messageTypes, TestFinishedEvent)
            && message.Dispatch(messageTypes, TestMethodCleanupFailureEvent)
            && message.Dispatch(messageTypes, TestMethodFinishedEvent)
            && message.Dispatch(messageTypes, TestMethodStartingEvent)
            && message.Dispatch(messageTypes, TestOutputEvent)
            && message.Dispatch(messageTypes, TestPassedEvent)
            && message.Dispatch(messageTypes, TestSkippedEvent)
            && message.Dispatch(messageTypes, TestStartingEvent);
    }
}
