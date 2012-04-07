using System.Collections.Generic;
using System.Reflection;

namespace Xunit.Sdk
{
    /// <summary>
    /// Command used to wrap a <see cref="ITestCommand"/> which has associated
    /// fixture data.
    /// </summary>
    public class FixtureCommand : DelegatingTestCommand
    {
        readonly Dictionary<MethodInfo, object> fixtures;

        /// <summary>
        /// Creates a new instance of the <see cref="FixtureCommand"/> class.
        /// </summary>
        /// <param name="innerCommand">The inner command</param>
        /// <param name="fixtures">The fixtures to be set on the test class</param>
        public FixtureCommand(ITestCommand innerCommand, Dictionary<MethodInfo, object> fixtures)
            : base(innerCommand)
        {
            this.fixtures = fixtures;
        }

        /// <summary>
        /// Sets the fixtures on the test class by calling SetFixture, then
        /// calls the inner command.
        /// </summary>
        /// <param name="testClass">The instance of the test class</param>
        /// <returns>Returns information about the test run</returns>
        public override MethodResult Execute(object testClass)
        {
            try
            {
                if (testClass != null)
                    foreach (KeyValuePair<MethodInfo, object> fixture in fixtures)
                        fixture.Key.Invoke(testClass, new object[] { fixture.Value });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionUtility.RethrowWithNoStackTraceLoss(ex.InnerException);
            }

            return InnerCommand.Execute(testClass);
        }
    }
}