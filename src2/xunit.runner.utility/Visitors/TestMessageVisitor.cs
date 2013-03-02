using System;
using System.Security;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit
{
    public abstract class TestMessageVisitor : MarshalByRefObject, IMessageSink
    {
        public virtual void Dispose() { }

        void DoVisit<TMessage>(ITestMessage message, Action<TMessage> callback)
            where TMessage : class, ITestMessage
        {
            TMessage castMessage = message as TMessage;
            if (castMessage != null)
                callback(castMessage);
        }

        public virtual void OnMessage(ITestMessage message)
        {
            DoVisit<IAfterTestFinished>(message, Visit);
            DoVisit<IAfterTestStarting>(message, Visit);
            DoVisit<IBeforeTestFinished>(message, Visit);
            DoVisit<IBeforeTestStarting>(message, Visit);
            DoVisit<IDiscoveryCompleteMessage>(message, Visit);
            DoVisit<IErrorMessage>(message, Visit);
            DoVisit<ITestAssemblyFinished>(message, Visit);
            DoVisit<ITestAssemblyStarting>(message, Visit);
            DoVisit<ITestCaseDiscoveryMessage>(message, Visit);
            DoVisit<ITestCaseFinished>(message, Visit);
            DoVisit<ITestCaseStarting>(message, Visit);
            DoVisit<ITestClassConstructionFinished>(message, Visit);
            DoVisit<ITestClassConstructionStarting>(message, Visit);
            DoVisit<ITestClassDisposeFinished>(message, Visit);
            DoVisit<ITestClassDisposeStarting>(message, Visit);
            DoVisit<ITestClassFinished>(message, Visit);
            DoVisit<ITestClassStarting>(message, Visit);
            DoVisit<ITestCollectionFinished>(message, Visit);
            DoVisit<ITestCollectionStarting>(message, Visit);
            DoVisit<ITestFailed>(message, Visit);
            DoVisit<ITestFinished>(message, Visit);
            DoVisit<ITestMethodFinished>(message, Visit);
            DoVisit<ITestMethodStarting>(message, Visit);
            DoVisit<ITestPassed>(message, Visit);
            DoVisit<ITestSkipped>(message, Visit);
            DoVisit<ITestStarting>(message, Visit);
        }

        protected virtual void Visit(IAfterTestFinished afterTestFinished) { }

        protected virtual void Visit(IAfterTestStarting afterTestStarting) { }

        protected virtual void Visit(IBeforeTestFinished beforeTestFinished) { }

        protected virtual void Visit(IBeforeTestStarting beforeTestStarting) { }

        protected virtual void Visit(IDiscoveryCompleteMessage discoveryComplete) { }

        protected virtual void Visit(IErrorMessage error) { }

        protected virtual void Visit(ITestAssemblyFinished assemblyFinished) { }

        protected virtual void Visit(ITestAssemblyStarting assemblyStarting) { }

        protected virtual void Visit(ITestCaseDiscoveryMessage testCaseDiscovered) { }

        protected virtual void Visit(ITestCaseFinished testCaseFinished) { }

        protected virtual void Visit(ITestCaseStarting testCaseStarting) { }

        protected virtual void Visit(ITestClassConstructionFinished testClassConstructionFinished) { }

        protected virtual void Visit(ITestClassConstructionStarting testClassConstructionStarting) { }

        protected virtual void Visit(ITestClassDisposeFinished testClassDisposedFinished) { }

        protected virtual void Visit(ITestClassDisposeStarting testClassDisposeStarting) { }

        protected virtual void Visit(ITestClassFinished testClassFinished) { }

        protected virtual void Visit(ITestClassStarting testClassStarting) { }

        protected virtual void Visit(ITestCollectionFinished testCollectionFinished) { }

        protected virtual void Visit(ITestCollectionStarting testCollectionStarting) { }

        protected virtual void Visit(ITestFailed testFailed) { }

        protected virtual void Visit(ITestFinished testFinished) { }

        protected virtual void Visit(ITestMethodFinished testMethodFinished) { }

        protected virtual void Visit(ITestMethodStarting testMethodStarting) { }

        protected virtual void Visit(ITestPassed testPassed) { }

        protected virtual void Visit(ITestSkipped testSkipped) { }

        protected virtual void Visit(ITestStarting testStarting) { }

        /// <summary/>
        [SecurityCritical]
        public override Object InitializeLifetimeService()
        {
            return null;
        }
    }

    public abstract class TestMessageVisitor<TCompleteMessage> : TestMessageVisitor
        where TCompleteMessage : ITestMessage
    {
        protected TestMessageVisitor()
        {
            Finished = new ManualResetEvent(initialState: false);
        }

        public ManualResetEvent Finished { get; private set; }

        public override void Dispose()
        {
            ((IDisposable)Finished).Dispose();
        }

        public override void OnMessage(ITestMessage message)
        {
            base.OnMessage(message);

            if (message is TCompleteMessage)
                Finished.Set();
        }
    }
}