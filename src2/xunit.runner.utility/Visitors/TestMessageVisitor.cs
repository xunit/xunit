using System;
using System.Security;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit
{
    public class TestMessageVisitor : MarshalByRefObject, IMessageSink
    {
        public virtual void Dispose() { }

        bool DoVisit<TMessage>(ITestMessage message, Func<TMessage, bool> callback)
            where TMessage : class, ITestMessage
        {
            TMessage castMessage = message as TMessage;
            if (castMessage != null)
                return callback(castMessage);

            return true;
        }

        public virtual bool OnMessage(ITestMessage message)
        {
            return
                DoVisit<IAfterTestFinished>(message, Visit) &&
                DoVisit<IAfterTestStarting>(message, Visit) &&
                DoVisit<IBeforeTestFinished>(message, Visit) &&
                DoVisit<IBeforeTestStarting>(message, Visit) &&
                DoVisit<IDiscoveryCompleteMessage>(message, Visit) &&
                DoVisit<IErrorMessage>(message, Visit) &&
                DoVisit<ITestAssemblyFinished>(message, Visit) &&
                DoVisit<ITestAssemblyStarting>(message, Visit) &&
                DoVisit<ITestCaseDiscoveryMessage>(message, Visit) &&
                DoVisit<ITestCaseFinished>(message, Visit) &&
                DoVisit<ITestCaseStarting>(message, Visit) &&
                DoVisit<ITestClassConstructionFinished>(message, Visit) &&
                DoVisit<ITestClassConstructionStarting>(message, Visit) &&
                DoVisit<ITestClassDisposeFinished>(message, Visit) &&
                DoVisit<ITestClassDisposeStarting>(message, Visit) &&
                DoVisit<ITestClassFinished>(message, Visit) &&
                DoVisit<ITestClassStarting>(message, Visit) &&
                DoVisit<ITestCollectionFinished>(message, Visit) &&
                DoVisit<ITestCollectionStarting>(message, Visit) &&
                DoVisit<ITestFailed>(message, Visit) &&
                DoVisit<ITestFinished>(message, Visit) &&
                DoVisit<ITestMethodFinished>(message, Visit) &&
                DoVisit<ITestMethodStarting>(message, Visit) &&
                DoVisit<ITestPassed>(message, Visit) &&
                DoVisit<ITestSkipped>(message, Visit) &&
                DoVisit<ITestStarting>(message, Visit);
        }

        protected virtual bool Visit(IAfterTestFinished afterTestFinished)
        {
            return true;
        }

        protected virtual bool Visit(IAfterTestStarting afterTestStarting)
        {
            return true;
        }

        protected virtual bool Visit(IBeforeTestFinished beforeTestFinished)
        {
            return true;
        }

        protected virtual bool Visit(IBeforeTestStarting beforeTestStarting)
        {
            return true;
        }

        protected virtual bool Visit(IDiscoveryCompleteMessage discoveryComplete)
        {
            return true;
        }

        protected virtual bool Visit(IErrorMessage error)
        {
            return true;
        }

        protected virtual bool Visit(ITestAssemblyFinished assemblyFinished)
        {
            return true;
        }

        protected virtual bool Visit(ITestAssemblyStarting assemblyStarting)
        {
            return true;
        }

        protected virtual bool Visit(ITestCaseDiscoveryMessage testCaseDiscovered)
        {
            return true;
        }

        protected virtual bool Visit(ITestCaseFinished testCaseFinished)
        {
            return true;
        }

        protected virtual bool Visit(ITestCaseStarting testCaseStarting)
        {
            return true;
        }

        protected virtual bool Visit(ITestClassConstructionFinished testClassConstructionFinished)
        {
            return true;
        }

        protected virtual bool Visit(ITestClassConstructionStarting testClassConstructionStarting)
        {
            return true;
        }

        protected virtual bool Visit(ITestClassDisposeFinished testClassDisposedFinished)
        {
            return true;
        }

        protected virtual bool Visit(ITestClassDisposeStarting testClassDisposeStarting)
        {
            return true;
        }

        protected virtual bool Visit(ITestClassFinished testClassFinished)
        {
            return true;
        }

        protected virtual bool Visit(ITestClassStarting testClassStarting)
        {
            return true;
        }

        protected virtual bool Visit(ITestCollectionFinished testCollectionFinished)
        {
            return true;
        }

        protected virtual bool Visit(ITestCollectionStarting testCollectionStarting)
        {
            return true;
        }

        protected virtual bool Visit(ITestFailed testFailed)
        {
            return true;
        }

        protected virtual bool Visit(ITestFinished testFinished)
        {
            return true;
        }

        protected virtual bool Visit(ITestMethodFinished testMethodFinished)
        {
            return true;
        }

        protected virtual bool Visit(ITestMethodStarting testMethodStarting)
        {
            return true;
        }

        protected virtual bool Visit(ITestPassed testPassed)
        {
            return true;
        }

        protected virtual bool Visit(ITestSkipped testSkipped)
        {
            return true;
        }

        protected virtual bool Visit(ITestStarting testStarting)
        {
            return true;
        }

        /// <summary/>
        [SecurityCritical]
        public override Object InitializeLifetimeService()
        {
            return null;
        }
    }

    public class TestMessageVisitor<TCompleteMessage> : TestMessageVisitor
        where TCompleteMessage : ITestMessage
    {
        public TestMessageVisitor()
        {
            Finished = new ManualResetEvent(initialState: false);
        }

        public ManualResetEvent Finished { get; private set; }

        public override void Dispose()
        {
            ((IDisposable)Finished).Dispose();
        }

        public override bool OnMessage(ITestMessage message)
        {
            var result = base.OnMessage(message);

            if (message is TCompleteMessage)
                Finished.Set();

            return result;
        }
    }
}