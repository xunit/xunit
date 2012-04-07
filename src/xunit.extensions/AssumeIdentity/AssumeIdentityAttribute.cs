using System;
using System.Reflection;
using System.Security.Principal;
using System.Threading;

namespace Xunit.Extensions
{
    /// <summary>
    /// Apply this attribute to your test method to replace the 
    /// <see cref="Thread.CurrentPrincipal"/> with another role.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class AssumeIdentityAttribute : BeforeAfterTestAttribute
    {
        readonly string name;
        IPrincipal originalPrincipal;

        /// <summary>
        /// Replaces the identity of the current thread with <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The role's name</param>
        public AssumeIdentityAttribute(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be null or empty", "name");

            this.name = name;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Restores the original <see cref="Thread.CurrentPrincipal"/>.
        /// </summary>
        /// <param name="methodUnderTest">The method under test</param>
        public override void After(MethodInfo methodUnderTest)
        {
            Thread.CurrentPrincipal = originalPrincipal;
        }

        /// <summary>
        /// Stores the current <see cref="Thread.CurrentPrincipal"/> and replaces it with
        /// a new role identified in constructor.
        /// </summary>
        /// <param name="methodUnderTest">The method under test</param>
        public override void Before(MethodInfo methodUnderTest)
        {
            originalPrincipal = Thread.CurrentPrincipal;
            GenericIdentity identity = new GenericIdentity("xUnit");
            GenericPrincipal principal = new GenericPrincipal(identity, new string[] { name });
            Thread.CurrentPrincipal = principal;
        }
    }
}