using System;
using System.Reflection;

namespace Xunit
{
    /// <summary>
    /// Base attribute which indicates a test method interception (allows code to be run before and
    /// after the test is run).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class BeforeAfterTestAttribute : Attribute
    {
        object typeId = new object();

        /// <summary>
        /// This method is called after the test method is executed.
        /// </summary>
        /// <param name="methodUnderTest">The method under test</param>
        public virtual void After(MethodInfo methodUnderTest) { }

        /// <summary>
        /// This method is called before the test method is executed.
        /// </summary>
        /// <param name="methodUnderTest">The method under test</param>
        public virtual void Before(MethodInfo methodUnderTest) { }

        /// <inheritdoc/>
        public override object TypeId
        {
            get { return typeId; }
        }
    }
}