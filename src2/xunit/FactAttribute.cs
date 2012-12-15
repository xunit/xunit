using System;
using System.Diagnostics.CodeAnalysis;
using Xunit.Sdk;

namespace Xunit
{
    [XunitDiscoverer(DiscovererType = typeof(FactDiscoverer))]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is designed as an extensibility point.")]
    public class FactAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the test to be used when the test is skipped. Defaults to
        /// null, which will cause the fully qualified test name to be used.
        /// </summary>
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// Marks the test so that it will not be run, and gets or sets the skip reason
        /// </summary>
        public virtual string Skip { get; set; }

        /// <summary>
        /// Marks the test as failing if it does not finish running within the given time
        /// period, in milliseconds; set to 0 or less to indicate the method has no timeout
        /// </summary>
        public virtual int Timeout { get; set; }
    }
}
