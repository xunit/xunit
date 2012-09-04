using Xunit.Abstractions;

namespace Xunit.Sdk2
{
    /// <summary>
    /// Represents a test case which is associated with a class.
    /// </summary>
    public interface IClassTestCase : ITestCase
    {
        /// <summary>
        /// Gets the class that this test case is attached to.
        /// </summary>
        ITypeInfo Class { get; }
    }
}
