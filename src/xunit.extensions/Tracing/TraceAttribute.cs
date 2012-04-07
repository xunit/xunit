using System;
using System.Diagnostics;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Extensions
{
    /// <summary>
    /// Apply to a test method to trace the method begin and end.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TraceAttribute : BeforeAfterTestAttribute
    {
        /// <summary>
        /// This method is called before the test method is executed.
        /// </summary>
        /// <param name="methodUnderTest">The method under test</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This is validated elsewhere.")]
        public override void Before(MethodInfo methodUnderTest)
        {
            Trace.WriteLine(String.Format("Before : {0}.{1}", methodUnderTest.DeclaringType.FullName, methodUnderTest.Name));
        }

        /// <summary>
        /// This method is called after the test method is executed.
        /// </summary>
        /// <param name="methodUnderTest">The method under test</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This is validated elsewhere.")]
        public override void After(MethodInfo methodUnderTest)
        {
            Trace.WriteLine(String.Format("After : {0}.{1}", methodUnderTest.DeclaringType.FullName, methodUnderTest.Name));
        }
    }
}