using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IMessageSinkWithTypes"/> that provides events that can be used to register
    /// handlers for specific message types without the burden of casting.
    /// </summary>
    public class TestMessageVisitor2 : LongLivedMarshalByRefObject, IMessageSinkWithTypes
    {
        /// <summary>
        /// Occurs when a <see cref="ITestAssemblyDiscoveryFinished"/> message is received.
        /// </summary>
        public event MessageHandler<ITestAssemblyDiscoveryFinished> TestAssemblyDiscoveryFinishedEvent;
        /// <summary>
        /// Occurs when a <see cref="ITestAssemblyDiscoveryStarting"/> message is received.
        /// </summary>
        public event MessageHandler<ITestAssemblyDiscoveryStarting> TestAssemblyDiscoveryStartingEvent;
        /// <summary>
        /// Occurs when a <see cref="ITestAssemblyExecutionFinished"/> message is received.
        /// </summary>
        public event MessageHandler<ITestAssemblyExecutionFinished> TestAssemblyExecutionFinishedEvent;
        /// <summary>
        /// Occurs when a <see cref="ITestAssemblyExecutionStarting"/> message is received.
        /// </summary>
        public event MessageHandler<ITestAssemblyExecutionStarting> TestAssemblyExecutionStartingEvent;
        /// <summary>
        /// Occurs when a <see cref="ITestExecutionSummary"/> message is received.
        /// </summary>
        public event MessageHandler<ITestExecutionSummary> TestExecutionSummaryEvent;
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
        /// Occurs when a <see cref="IDiagnosticMessage"/> message is received.
        /// </summary>
        public event MessageHandler<IDiagnosticMessage> DiagnosticMessageEvent;
        /// <summary>
        /// Occurs when a <see cref="IDiscoveryCompleteMessage"/> message is received.
        /// </summary>
        public event MessageHandler<IDiscoveryCompleteMessage> DiscoveryCompleteMessageEvent;
        /// <summary>
        /// Occurs when a <see cref="IErrorMessage"/> message is received.
        /// </summary>
        public event MessageHandler<IErrorMessage> ErrorMessageEvent;
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
        /// Occurs when a <see cref="ITestCaseDiscoveryMessage"/> message is received.
        /// </summary>
        public event MessageHandler<ITestCaseDiscoveryMessage> TestCaseDiscoveryMessageEvent;
        /// <summary>
        /// Occurs when a <see cref="ITestCaseFinished"/> message is received.
        /// </summary>
        public event MessageHandler<ITestCaseFinished> TestCaseFinishedEvent;
        /// <summary>
        /// Occurs when a <see cref="ITestOutput"/> message is received.
        /// </summary>
        public event MessageHandler<ITestOutput> TestOutputEvent;
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

        bool DoVisit<TMessage>(IMessageSinkMessage message, HashSet<string> types, MessageHandler<TMessage> callback)
            where TMessage : class, IMessageSinkMessage
        {
            if (callback != null)
            {
                TMessage castMessage = null;
                if (types == null)
                {
                    castMessage = message as TMessage;
                }
                else if (types.Contains(typeof(TMessage).FullName))
                {
                    castMessage = message as TMessage;
                }
                if (castMessage != null)
                {
                    var args = new MessageHandlerArgs<TMessage>(castMessage);
                    callback(args);
                    return !args.IsStopped;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        bool IMessageSink.OnMessage(IMessageSinkMessage message)
        {
            string[] types = null;
#if PLATFORM_DOTNET
            types = GetMessageTypes(message);
#else
            if (!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(message))
            {
                types = GetMessageTypes(message);
            }
#endif
            return OnMessageWithTypes(message, types);
        }

        static string[] GetMessageTypes(IMessageSinkMessage message)
        {
            return message.GetType().GetInterfaces().Select(i => i.FullName).ToArray();
        }

        /// <inheritdoc/>
        public virtual bool OnMessageWithTypes(IMessageSinkMessage message, string[] messageTypes)
        {
            HashSet<string> types = null;
            if (messageTypes != null)
                types = new HashSet<string>(messageTypes, StringComparer.OrdinalIgnoreCase);
            var b = DoVisit(message, types, TestAssemblyDiscoveryFinishedEvent);
            b = b && DoVisit(message, types, TestAssemblyDiscoveryStartingEvent);
            b = b && DoVisit(message, types, TestAssemblyExecutionFinishedEvent);
            b = b && DoVisit(message, types, TestAssemblyExecutionStartingEvent);
            b = b && DoVisit(message, types, TestExecutionSummaryEvent);
            b = b && DoVisit(message, types, AfterTestFinishedEvent);
            b = b && DoVisit(message, types, AfterTestStartingEvent);
            b = b && DoVisit(message, types, BeforeTestFinishedEvent);
            b = b && DoVisit(message, types, BeforeTestStartingEvent);
            b = b && DoVisit(message, types, DiagnosticMessageEvent);
            b = b && DoVisit(message, types, DiscoveryCompleteMessageEvent);
            b = b && DoVisit(message, types, ErrorMessageEvent);
            b = b && DoVisit(message, types, TestAssemblyCleanupFailureEvent);
            b = b && DoVisit(message, types, TestAssemblyFinishedEvent);
            b = b && DoVisit(message, types, TestAssemblyStartingEvent);
            b = b && DoVisit(message, types, TestCaseCleanupFailureEvent);
            b = b && DoVisit(message, types, TestCaseDiscoveryMessageEvent);
            b = b && DoVisit(message, types, TestCaseFinishedEvent);
            b = b && DoVisit(message, types, TestOutputEvent);
            b = b && DoVisit(message, types, TestCaseStartingEvent);
            b = b && DoVisit(message, types, TestClassCleanupFailureEvent);
            b = b && DoVisit(message, types, TestClassConstructionFinishedEvent);
            b = b && DoVisit(message, types, TestClassConstructionStartingEvent);
            b = b && DoVisit(message, types, TestClassDisposeFinishedEvent);
            b = b && DoVisit(message, types, TestClassDisposeStartingEvent);
            b = b && DoVisit(message, types, TestClassFinishedEvent);
            b = b && DoVisit(message, types, TestClassStartingEvent);
            b = b && DoVisit(message, types, TestCleanupFailureEvent);
            b = b && DoVisit(message, types, TestCollectionCleanupFailureEvent);
            b = b && DoVisit(message, types, TestCollectionFinishedEvent);
            b = b && DoVisit(message, types, TestCollectionStartingEvent);
            b = b && DoVisit(message, types, TestFailedEvent);
            b = b && DoVisit(message, types, TestFinishedEvent);
            b = b && DoVisit(message, types, TestMethodCleanupFailureEvent);
            b = b && DoVisit(message, types, TestMethodFinishedEvent);
            b = b && DoVisit(message, types, TestMethodStartingEvent);
            b = b && DoVisit(message, types, TestPassedEvent);
            b = b && DoVisit(message, types, TestSkippedEvent);
            b = b && DoVisit(message, types, TestStartingEvent);
            return b;
        }
    }
}
