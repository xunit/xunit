using System;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IMessageSink"/> that provides several Visit methods that
    /// can provide access to specific message types without the burden of casting.
    /// </summary>
    [Obsolete("This class has poor performance; please use TestMessageSink instead.")]
    public class TestMessageVisitor : LongLivedMarshalByRefObject, IMessageSink
    {
        /// <summary>
        /// Dispatches the message to the given callback, if it's of the correct type.
        /// </summary>
        /// <typeparam name="TMessage">The message type</typeparam>
        /// <param name="message">The message</param>
        /// <param name="callback">The callback</param>
        /// <returns>The result of the callback, if called; <c>true</c>, otherwise</returns>
        protected static bool DoVisit<TMessage>(IMessageSinkMessage message, Func<TMessage, bool> callback)
            where TMessage : class, IMessageSinkMessage
        {
            var castMessage = message as TMessage;
            if (castMessage != null)
                return callback(castMessage);

            return true;
        }

        /// <summary>
        /// Dispatches the message to the given callback, if it's of the correct type.
        /// The callback is provided with both the message and this instance of the visitor.
        /// </summary>
        /// <typeparam name="TMessage">The message type</typeparam>
        /// <param name="message">The message</param>
        /// <param name="callback">The callback</param>
        /// <returns>The result of the callback, if called; <c>true</c>, otherwise</returns>
        bool DoVisit<TMessage>(IMessageSinkMessage message, Func<TestMessageVisitor, TMessage, bool> callback)
            where TMessage : class, IMessageSinkMessage
        {
            var castMessage = message as TMessage;
            if (castMessage != null)
                return callback(this, castMessage);

            return true;
        }

        /// <inheritdoc/>
        public virtual bool OnMessage(IMessageSinkMessage message)
        {
            return
                DoVisit<ITestAssemblyDiscoveryFinished>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestAssemblyDiscoveryStarting>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestAssemblyExecutionFinished>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestAssemblyExecutionStarting>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestExecutionSummary>(message, (t, m) => t.Visit(m)) &&
                DoVisit<IAfterTestFinished>(message, (t, m) => t.Visit(m)) &&
                DoVisit<IAfterTestStarting>(message, (t, m) => t.Visit(m)) &&
                DoVisit<IBeforeTestFinished>(message, (t, m) => t.Visit(m)) &&
                DoVisit<IBeforeTestStarting>(message, (t, m) => t.Visit(m)) &&
                DoVisit<IDiagnosticMessage>(message, (t, m) => t.Visit(m)) &&
                DoVisit<IDiscoveryCompleteMessage>(message, (t, m) => t.Visit(m)) &&
                DoVisit<IErrorMessage>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestAssemblyCleanupFailure>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestAssemblyFinished>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestAssemblyStarting>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestCaseCleanupFailure>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestCaseDiscoveryMessage>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestCaseFinished>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestOutput>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestCaseStarting>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestClassCleanupFailure>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestClassConstructionFinished>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestClassConstructionStarting>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestClassDisposeFinished>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestClassDisposeStarting>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestClassFinished>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestClassStarting>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestCleanupFailure>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestCollectionCleanupFailure>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestCollectionFinished>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestCollectionStarting>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestFailed>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestFinished>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestMethodCleanupFailure>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestMethodFinished>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestMethodStarting>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestPassed>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestSkipped>(message, (t, m) => t.Visit(m)) &&
                DoVisit<ITestStarting>(message, (t, m) => t.Visit(m));
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestAssemblyDiscoveryFinished"/> is sent to the message sink.
        /// </summary>
        /// <param name="discoveryFinished">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestAssemblyDiscoveryFinished discoveryFinished)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestAssemblyDiscoveryStarting"/> is sent to the message sink.
        /// </summary>
        /// <param name="discoveryStarting">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestAssemblyDiscoveryStarting discoveryStarting)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestAssemblyExecutionFinished"/> is sent to the message sink.
        /// </summary>
        /// <param name="executionFinished">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestAssemblyExecutionFinished executionFinished)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestAssemblyExecutionStarting"/> is sent to the message sink.
        /// </summary>
        /// <param name="executionStarting">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestAssemblyExecutionStarting executionStarting)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestExecutionSummary"/> is sent to the message sink.
        /// </summary>
        /// <param name="executionSummary">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestExecutionSummary executionSummary)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="IAfterTestFinished"/> is sent to the message sink.
        /// </summary>
        /// <param name="afterTestFinished">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(IAfterTestFinished afterTestFinished)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="IAfterTestStarting"/> is sent to the message sink.
        /// </summary>
        /// <param name="afterTestStarting">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(IAfterTestStarting afterTestStarting)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="IBeforeTestFinished"/> is sent to the message sink.
        /// </summary>
        /// <param name="beforeTestFinished">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(IBeforeTestFinished beforeTestFinished)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="IBeforeTestStarting"/> is sent to the message sink.
        /// </summary>
        /// <param name="beforeTestStarting">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(IBeforeTestStarting beforeTestStarting)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="IDiagnosticMessage"/> is sent to the message sink.
        /// </summary>
        /// <param name="diagnosticMessage">The message.</param>
        /// <returns>Return <c>true</c> to continue discovering/executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(IDiagnosticMessage diagnosticMessage)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="IDiscoveryCompleteMessage"/> is sent to the message sink.
        /// </summary>
        /// <param name="discoveryComplete">The message.</param>
        /// <returns>Return <c>true</c> to continue discovering tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(IDiscoveryCompleteMessage discoveryComplete)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="IErrorMessage"/> is sent to the message sink.
        /// </summary>
        /// <param name="error">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(IErrorMessage error)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestAssemblyCleanupFailure"/> is sent to the message sink.
        /// </summary>
        /// <param name="cleanupFailure">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestAssemblyCleanupFailure cleanupFailure)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestAssemblyFinished"/> is sent to the message sink.
        /// </summary>
        /// <param name="assemblyFinished">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestAssemblyFinished assemblyFinished)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestAssemblyStarting"/> is sent to the message sink.
        /// </summary>
        /// <param name="assemblyStarting">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestAssemblyStarting assemblyStarting)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestCaseCleanupFailure"/> is sent to the message sink.
        /// </summary>
        /// <param name="cleanupFailure">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestCaseCleanupFailure cleanupFailure)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestCaseDiscoveryMessage"/> is sent to the message sink.
        /// </summary>
        /// <param name="testCaseDiscovered">The message.</param>
        /// <returns>Return <c>true</c> to continue discovering tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestCaseDiscoveryMessage testCaseDiscovered)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestCaseFinished"/> is sent to the message sink.
        /// </summary>
        /// <param name="testCaseFinished">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestCaseFinished testCaseFinished)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestOutput"/> is sent to the message sink.
        /// </summary>
        /// <param name="testCaseOutput">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestOutput testCaseOutput)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestCaseStarting"/> is sent to the message sink.
        /// </summary>
        /// <param name="testCaseStarting">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestCaseStarting testCaseStarting)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestClassCleanupFailure"/> is sent to the message sink.
        /// </summary>
        /// <param name="cleanupFailure">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestClassCleanupFailure cleanupFailure)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestClassConstructionFinished"/> is sent to the message sink.
        /// </summary>
        /// <param name="testClassConstructionFinished">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestClassConstructionFinished testClassConstructionFinished)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestClassConstructionStarting"/> is sent to the message sink.
        /// </summary>
        /// <param name="testClassConstructionStarting">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestClassConstructionStarting testClassConstructionStarting)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestClassDisposeFinished"/> is sent to the message sink.
        /// </summary>
        /// <param name="testClassDisposedFinished">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestClassDisposeFinished testClassDisposedFinished)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestClassDisposeStarting"/> is sent to the message sink.
        /// </summary>
        /// <param name="testClassDisposeStarting">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestClassDisposeStarting testClassDisposeStarting)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestClassFinished"/> is sent to the message sink.
        /// </summary>
        /// <param name="testClassFinished">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestClassFinished testClassFinished)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestClassStarting"/> is sent to the message sink.
        /// </summary>
        /// <param name="testClassStarting">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestClassStarting testClassStarting)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestCleanupFailure"/> is sent to the message sink.
        /// </summary>
        /// <param name="cleanupFailure">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestCleanupFailure cleanupFailure)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestCollectionCleanupFailure"/> is sent to the message sink.
        /// </summary>
        /// <param name="cleanupFailure">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestCollectionCleanupFailure cleanupFailure)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestCollectionFinished"/> is sent to the message sink.
        /// </summary>
        /// <param name="testCollectionFinished">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestCollectionFinished testCollectionFinished)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestCollectionStarting"/> is sent to the message sink.
        /// </summary>
        /// <param name="testCollectionStarting">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestCollectionStarting testCollectionStarting)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestFailed"/> is sent to the message sink.
        /// </summary>
        /// <param name="testFailed">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestFailed testFailed)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestFinished"/> is sent to the message sink.
        /// </summary>
        /// <param name="testFinished">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestFinished testFinished)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestMethodCleanupFailure"/> is sent to the message sink.
        /// </summary>
        /// <param name="cleanupFailure">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestMethodCleanupFailure cleanupFailure)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestMethodFinished"/> is sent to the message sink.
        /// </summary>
        /// <param name="testMethodFinished">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestMethodFinished testMethodFinished)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestMethodStarting"/> is sent to the message sink.
        /// </summary>
        /// <param name="testMethodStarting">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestMethodStarting testMethodStarting)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestPassed"/> is sent to the message sink.
        /// </summary>
        /// <param name="testPassed">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestPassed testPassed)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestSkipped"/> is sent to the message sink.
        /// </summary>
        /// <param name="testSkipped">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestSkipped testSkipped)
        {
            return true;
        }

        /// <summary>
        /// Called when an instance of <see cref="ITestStarting"/> is sent to the message sink.
        /// </summary>
        /// <param name="testStarting">The message.</param>
        /// <returns>Return <c>true</c> to continue executing tests; <c>false</c> otherwise.</returns>
        protected virtual bool Visit(ITestStarting testStarting)
        {
            return true;
        }
    }

    /// <summary>
    /// An implementation of <see cref="IMessageSink" /> that provides several Visit methods that
    /// can provide access to specific message types without the burden of casting. It also records
    /// when it sees a completion message, and sets the <see cref="Finished" /> event appropriately.
    /// </summary>
    /// <typeparam name="TCompleteMessage">The type of the completion message.</typeparam>
    [Obsolete("This class has poor performance; please use TestMessageSink instead.")]
    public class TestMessageVisitor<TCompleteMessage> : TestMessageVisitor, IDisposable
        where TCompleteMessage : IMessageSinkMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestMessageVisitor{TCompleteMessage}"/> class.
        /// </summary>
        public TestMessageVisitor()
        {
            Finished = new ManualResetEvent(false);
        }

        /// <summary>
        /// This event is triggered when the completion message has been seen.
        /// </summary>
        public ManualResetEvent Finished { get; private set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            ((IDisposable)Finished).Dispose();
        }

        /// <inheritdoc/>
        public override bool OnMessage(IMessageSinkMessage message)
        {
            var result = base.OnMessage(message);

            if (message is TCompleteMessage)
                Finished.Set();

            return result;
        }
    }
}
