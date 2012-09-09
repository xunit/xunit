using System.Collections.Generic;

namespace Xunit.Abstractions
{
    public interface ITestCase
    {
        /// <summary>
        /// Gets the assembly this test case belongs to.
        /// </summary>
        IAssemblyInfo Assembly { get; }

        /// <summary>
        /// Gets the display name of the test method.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the test collection this test case belongs to.
        /// </summary>
        ITestCollection TestCollection { get; }

        /// <summary>
        /// Gets the trait values associated with this test case. If
        /// there are none, or the framework does not support traits,
        /// this should return an empty dictionary (not null). This
        /// dictionary should be treated as read-only.
        /// </summary>
        IDictionary<string, string> Traits { get; }
    }
}
