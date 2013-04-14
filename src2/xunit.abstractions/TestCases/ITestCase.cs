using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents a single test case in the system. This test case usually represents a single test, but in
    /// the case of dynamically generated data for data driven tests, the test case may actually return
    /// multiple results when run.
    /// </summary>
    public interface ITestCase : ISerializable
    {
        /// <summary>
        /// Gets the class that this test case is attached to.
        /// </summary>
        ITypeInfo Class { get; }

        /// <summary>
        /// Gets the display name of the test case.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the method associated with this test case.
        /// </summary>
        IMethodInfo Method { get; }

        /// <summary>
        /// Gets the display text for the reason a test is being skipped; if the test
        /// is not skipped, returns null.
        /// </summary>
        string SkipReason { get; }

        /// <summary>
        /// Get the source file line where the test is defined, if requested (and known).
        /// </summary>
        int? SourceFileLine { get; }

        /// <summary>
        /// Gets the source file name where the test is defined, if requested (and known).
        /// </summary>
        string SourceFileName { get; }

        /// <summary>
        /// Gets the test collection this test case belongs to.
        /// </summary>
        ITestCollection TestCollection { get; }

        /// <summary>
        /// Gets the trait values associated with this test case. If
        /// there are none, or the framework does not support traits,
        /// this should return an empty dictionary (not <c>null</c>). This
        /// dictionary must be treated as read-only.
        /// </summary>
        IDictionary<string, string> Traits { get; }
    }
}