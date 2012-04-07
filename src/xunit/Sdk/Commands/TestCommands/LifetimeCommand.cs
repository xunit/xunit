using System;
using System.Reflection;

namespace Xunit.Sdk
{
    /// <summary>
    /// Command that automatically creates the instance of the test class
    /// and disposes it (if it implements <see cref="IDisposable"/>).
    /// </summary>
    public class LifetimeCommand : DelegatingTestCommand
    {
        readonly IMethodInfo method;

        /// <summary>
        /// Creates a new instance of the <see cref="LifetimeCommand"/> object.
        /// </summary>
        /// <param name="innerCommand">The command that is bring wrapped</param>
        /// <param name="method">The method under test</param>
        public LifetimeCommand(ITestCommand innerCommand, IMethodInfo method)
            : base(innerCommand)
        {
            this.method = method;
        }

        /// <summary>
        /// Executes the test method. Creates a new instance of the class
        /// under tests and passes it to the inner command. Also catches
        /// any exceptions and converts them into <see cref="FailedResult"/>s.
        /// </summary>
        /// <param name="testClass">The instance of the test class</param>
        /// <returns>Returns information about the test run</returns>
        public override MethodResult Execute(object testClass)
        {
            try
            {
                if (testClass == null)
                    testClass = method.CreateInstance();
            }
            catch (TargetInvocationException ex)
            {
                ExceptionUtility.RethrowWithNoStackTraceLoss(ex.InnerException);
            }

            try
            {
                return InnerCommand.Execute(testClass);
            }
            finally
            {
                IDisposable disposable = testClass as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
        }
    }
}