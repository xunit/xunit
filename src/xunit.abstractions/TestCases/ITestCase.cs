using System.Collections.Generic;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents a single test case in the system. This test case usually represents a single test, but in
    /// the case of dynamically generated data for data driven tests, the test case may actually return
    /// multiple results when run.
    /// </summary>
    public interface ITestCase
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
        /// Get the source file name and line where the test is defined, if requested (and known).
        /// </summary>
        SourceInformation SourceInformation { get; }

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

        /// <summary>
        /// Gets a unique identifier for the test case.
        /// </summary>
        /// <remarks>
        /// The unique identifier for a test case should be able to discriminate
        /// among test cases, even those which are varied invocations against the
        /// same test method (i.e., theories). Ideally, this identifier would remain
        /// stable until such time as the developer changes some fundamental part
        /// of the identity (assembly, class name, test name, or test data); however,
        /// the minimum stability of the identifier must at least extend across
        /// multiple discoveries of the same test in the same (non-recompiled)
        /// assembly.
        /// </remarks>
        string UniqueID { get; }
    }
}