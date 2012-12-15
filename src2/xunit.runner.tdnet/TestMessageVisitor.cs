using Xunit.Abstractions;

namespace Xunit.Runner.TdNet
{
    public abstract class TestMessageVisitor : LongLivedMarshalByRefObject
    {
        protected virtual void Visit(ITestAssemblyFinished finished)
        {
        }

        protected virtual void Visit(ITestFailed failed)
        {
        }

        protected virtual void Visit(ITestPassed passed)
        {
        }

        protected virtual void Visit(ITestSkipped skipped)
        {
        }

        public virtual void Visit(ITestMessage testMessage)
        {
            ITestAssemblyFinished finished = testMessage as ITestAssemblyFinished;
            if (finished != null)
                Visit(finished);

            ITestFailed failed = testMessage as ITestFailed;
            if (failed != null)
                Visit(failed);

            ITestPassed passed = testMessage as ITestPassed;
            if (passed != null)
                Visit(passed);

            ITestSkipped skipped = testMessage as ITestSkipped;
            if (skipped != null)
                Visit(skipped);
        }
    }
}