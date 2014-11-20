using System;
using System.Threading;
using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// An implementation of <see cref="IMessageSink"/> that provides several Visit methods that
    /// can provide access to specific message types without the burden of casting.
    /// </summary>
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

        /// <inheritdoc/>
        public virtual bool OnMessage(IMessageSinkMessage message)
        {
            return
                DoVisit<IAfterTestFinished>(message, Visit) &&
                DoVisit<IAfterTestStarting>(message, Visit) &&
                DoVisit<IBeforeTestFinished>(message, Visit) &&
                DoVisit<IBeforeTestStarting>(message, Visit) &&
                DoVisit<IDiagnosticMessage>(message, Visit) &&
                DoVisit<IDiscoveryCompleteMessage>(message, Visit) &&
                DoVisit<IErrorMessage>(message, Visit) &&
                DoVisit<ITestAssemblyCleanupFailure>(message, Visit) &&
                DoVisit<ITestAssemblyFinished>(message, Visit) &&
                DoVisit<ITestAssemblyStarting>(message, Visit) &&
                DoVisit<ITestCaseCleanupFailure>(message, Visit) &&
                DoVisit<ITestCaseDiscoveryMessage>(message, Visit) &&
                DoVisit<ITestCaseFinished>(message, Visit) &&
                DoVisit<ITestOutput>(message, Visit) &&
                DoVisit<ITestCaseStarting>(message, Visit) &&
                DoVisit<ITestClassCleanupFailure>(message, Visit) &&
                DoVisit<ITestClassConstructionFinished>(message, Visit) &&
                DoVisit<ITestClassConstructionStarting>(message, Visit) &&
                DoVisit<ITestClassDisposeFinished>(message, Visit) &&
                DoVisit<ITestClassDisposeStarting>(message, Visit) &&
                DoVisit<ITestClassFinished>(message, Visit) &&
                DoVisit<ITestClassStarting>(message, Visit) &&
                DoVisit<ITestCleanupFailure>(message, Visit) &&
                DoVisit<ITestCollectionCleanupFailure>(message, Visit) &&
                DoVisit<ITestCollectionFinished>(message, Visit) &&
                DoVisit<ITestCollectionStarting>(message, Visit) &&
                DoVisit<ITestFailed>(message, Visit) &&
                DoVisit<ITestFinished>(message, Visit) &&
                DoVisit<ITestMethodCleanupFailure>(message, Visit) &&
                DoVisit<ITestMethodFinished>(message, Visit) &&
                DoVisit<ITestMethodStarting>(message, Visit) &&
                DoVisit<ITestPassed>(message, Visit) &&
                DoVisit<ITestSkipped>(message, Visit) &&
                DoVisit<ITestStarting>(message, Visit);
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
    public class TestMessageVisitor<TCompleteMessage> : TestMessageVisitor, IDisposable
        where TCompleteMessage : IMessageSinkMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestMessageVisitor{TCompleteMessage}"/> class.
        /// </summary>
        public TestMessageVisitor()
        {
            Finished = new ManualResetEvent(initialState: false);
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