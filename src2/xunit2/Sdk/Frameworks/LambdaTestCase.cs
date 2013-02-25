using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// A simple implementation of <see cref="XunitTestCase"/> wherein the running of the
    /// test case can be represented by an <see cref="Action"/>. Useful for emitting test
    /// cases which later evaluate to error messages (since throwing error messages during
    /// discovery is often the wrong thing to do). See <see cref="TheoryDiscoverer"/> for
    /// a use of this test case to emit an error message when a theory method is found
    /// that has no test data associated with it.
    /// </summary>
    public class LambdaTestCase : XunitTestCase
    {
        readonly Action lambda;

        public LambdaTestCase(IAssemblyInfo assembly, ITypeInfo testClass, IMethodInfo testMethod, IAttributeInfo factAttribute, Action lambda)
            : base(assembly, testClass, testMethod, factAttribute)
        {
            this.lambda = lambda;
        }

        protected override void RunTests(IMessageSink messageSink)
        {
            messageSink.OnMessage(new TestStarting { TestCase = this, TestDisplayName = DisplayName });

            try
            {
                lambda();
                messageSink.OnMessage(new TestPassed { TestCase = this, TestDisplayName = DisplayName });
            }
            catch (Exception ex)
            {
                messageSink.OnMessage(new TestFailed { TestCase = this, TestDisplayName = DisplayName, Exception = ex });
            }

            messageSink.OnMessage(new TestFinished { TestCase = this, TestDisplayName = DisplayName });
        }
    }
}