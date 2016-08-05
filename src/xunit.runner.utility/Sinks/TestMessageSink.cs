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
    public class TestMessageSink : LongLivedMarshalByRefObject, IMessageSinkWithTypes
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

        /// <summary>
        /// Attempts to optimally cast a message to the given message type, using the optional hash of
        /// interface types to improve casting performance.
        /// </summary>
        /// <typeparam name="TMessage">The desired destination message type.</typeparam>
        /// <param name="message">The message to test and cast.</param>
        /// <param name="types">The optional hash set of supported types.</param>
        /// <returns>The message as <typeparamref name="TMessage"/>, or <c>null</c>.</returns>
        protected TMessage Cast<TMessage>(IMessageSinkMessage message, HashSet<string> types) where TMessage : class, IMessageSinkMessage
            => types == null || types.Contains(typeof(TMessage).FullName) ? message as TMessage : null;

        bool HandleMessage<TMessage>(IMessageSinkMessage message, HashSet<string> types, MessageHandler<TMessage> callback)
            where TMessage : class, IMessageSinkMessage
        {
            if (callback != null)
            {
                var castMessage = Cast<TMessage>(message, types);
                if (castMessage != null)
                {
                    var args = new MessageHandlerArgs<TMessage>(castMessage);
                    callback(args);
                    return !args.IsStopped;
                }
            }

            return true;
        }

        bool IMessageSink.OnMessage(IMessageSinkMessage message)
        {
            string[] types = null;

#if PLATFORM_DOTNET
            types = GetMessageTypes(message);
#else
            if (!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(message))
                types = GetMessageTypes(message);
#endif

            return OnMessageWithTypes(message, types);
        }

        static string[] GetMessageTypes(IMessageSinkMessage message)
            => message.GetType().GetInterfaces().Select(i => i.FullName).ToArray();

        /// <summary>
        /// Gets the message types as a HashSet&lt;string&gt; to be used for faster lookups.
        /// </summary>
        /// <param name="messageTypes">The message type array.</param>
        /// <returns>The message types as a hash set..</returns>
        protected static HashSet<string> GetMessageTypesAsHashSet(string[] messageTypes)
            => messageTypes == null ? null : new HashSet<string>(messageTypes, StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public virtual bool OnMessageWithTypes(IMessageSinkMessage message, string[] messageTypes)
        {
            var types = GetMessageTypesAsHashSet(messageTypes);

            return HandleMessage(message, types, TestAssemblyDiscoveryFinishedEvent)
                && HandleMessage(message, types, TestAssemblyDiscoveryStartingEvent)
                && HandleMessage(message, types, TestAssemblyExecutionFinishedEvent)
                && HandleMessage(message, types, TestAssemblyExecutionStartingEvent)
                && HandleMessage(message, types, TestExecutionSummaryEvent)
                && HandleMessage(message, types, AfterTestFinishedEvent)
                && HandleMessage(message, types, AfterTestStartingEvent)
                && HandleMessage(message, types, BeforeTestFinishedEvent)
                && HandleMessage(message, types, BeforeTestStartingEvent)
                && HandleMessage(message, types, DiagnosticMessageEvent)
                && HandleMessage(message, types, DiscoveryCompleteMessageEvent)
                && HandleMessage(message, types, ErrorMessageEvent)
                && HandleMessage(message, types, TestAssemblyCleanupFailureEvent)
                && HandleMessage(message, types, TestAssemblyFinishedEvent)
                && HandleMessage(message, types, TestAssemblyStartingEvent)
                && HandleMessage(message, types, TestCaseCleanupFailureEvent)
                && HandleMessage(message, types, TestCaseDiscoveryMessageEvent)
                && HandleMessage(message, types, TestCaseFinishedEvent)
                && HandleMessage(message, types, TestOutputEvent)
                && HandleMessage(message, types, TestCaseStartingEvent)
                && HandleMessage(message, types, TestClassCleanupFailureEvent)
                && HandleMessage(message, types, TestClassConstructionFinishedEvent)
                && HandleMessage(message, types, TestClassConstructionStartingEvent)
                && HandleMessage(message, types, TestClassDisposeFinishedEvent)
                && HandleMessage(message, types, TestClassDisposeStartingEvent)
                && HandleMessage(message, types, TestClassFinishedEvent)
                && HandleMessage(message, types, TestClassStartingEvent)
                && HandleMessage(message, types, TestCleanupFailureEvent)
                && HandleMessage(message, types, TestCollectionCleanupFailureEvent)
                && HandleMessage(message, types, TestCollectionFinishedEvent)
                && HandleMessage(message, types, TestCollectionStartingEvent)
                && HandleMessage(message, types, TestFailedEvent)
                && HandleMessage(message, types, TestFinishedEvent)
                && HandleMessage(message, types, TestMethodCleanupFailureEvent)
                && HandleMessage(message, types, TestMethodFinishedEvent)
                && HandleMessage(message, types, TestMethodStartingEvent)
                && HandleMessage(message, types, TestPassedEvent)
                && HandleMessage(message, types, TestSkippedEvent)
                && HandleMessage(message, types, TestStartingEvent);
        }
    }
}
