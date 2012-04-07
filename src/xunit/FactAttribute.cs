using System;
using System.Collections.Generic;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Attribute that is applied to a method to indicate that it is a fact that should be run
    /// by the test runner. It can also be extended to support a customized definition of a
    /// test method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class FactAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the test to be used when the test is skipped. Defaults to
        /// null, which will cause the fully qualified test name to be used.
        /// </summary>
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// Obsolete. Please use the <see cref="DisplayName"/> property instead.
        /// </summary>
        [Obsolete("Please use the DisplayName property instead")]
        public virtual string Name
        {
            get { return DisplayName; }
            set { DisplayName = value; }
        }

        /// <summary>
        /// Marks the test so that it will not be run, and gets or sets the skip reason
        /// </summary>
        public virtual string Skip { get; set; }

        /// <summary>
        /// Marks the test as failing if it does not finish running within the given time
        /// period, in milliseconds; set to 0 or less to indicate the method has no timeout
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Creates instances of <see cref="ITestCommand"/> which represent individual intended
        /// invocations of the test method.
        /// </summary>
        /// <param name="method">The method under test</param>
        /// <returns>An enumerator through the desired test method invocations</returns>
        public IEnumerable<ITestCommand> CreateTestCommands(IMethodInfo method)
        {
            return EnumerateTestCommands(method);
        }

        /// <summary>
        /// Enumerates the test commands represented by this test method. Derived classes should
        /// override this method to return instances of <see cref="ITestCommand"/>, one per execution
        /// of a test method.
        /// </summary>
        /// <param name="method">The test method</param>
        /// <returns>The test commands which will execute the test runs for the given method</returns>
        protected virtual IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            yield return new FactCommand(method);
        }
    }
}