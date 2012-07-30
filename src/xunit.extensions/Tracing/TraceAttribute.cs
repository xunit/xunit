using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit.Extensions
{
    /// <summary>
    /// Apply to a test method to trace the method begin and end.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is designed as an extensibility point.")]
    public class TraceAttribute : BeforeAfterTestAttribute
    {
        /// <summary>
        /// This method is called before the test method is executed.
        /// </summary>
        /// <param name="methodUnderTest">The method under test</param>
        public override void Before(MethodInfo methodUnderTest)
        {
            Guard.ArgumentNotNull("methodUnderTest", methodUnderTest);

            Trace.WriteLine(String.Format(CultureInfo.CurrentCulture, "Before : {0}.{1}", methodUnderTest.DeclaringType.FullName, methodUnderTest.Name));
        }

        /// <summary>
        /// This method is called after the test method is executed.
        /// </summary>
        /// <param name="methodUnderTest">The method under test</param>
        public override void After(MethodInfo methodUnderTest)
        {
            Guard.ArgumentNotNull("methodUnderTest", methodUnderTest);

            Trace.WriteLine(String.Format(CultureInfo.CurrentCulture, "After : {0}.{1}", methodUnderTest.DeclaringType.FullName, methodUnderTest.Name));
        }
    }
}