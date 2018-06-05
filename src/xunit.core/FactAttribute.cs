using System;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Attribute that is applied to a method to indicate that it is a fact that should be run
    /// by the test runner. It can also be extended to support a customized definition of a
    /// test method.
    /// </summary>
    [XunitTestCaseDiscoverer("Xunit.Sdk.FactDiscoverer", "xunit.execution.{Platform}")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
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
        /// Marks the test as having a timeout, and gets or sets the timeout (in milliseconds).
        /// WARNING: Using this with parallelization turned on will result in undefined behavior.
        /// Timeout is only supported when parallelization is disabled, either globally or with
        /// a parallelization-disabled test collection.
        /// </summary>
        public virtual int Timeout { get; set; }
    }
}
