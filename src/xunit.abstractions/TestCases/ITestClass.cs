namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents a test class.
    /// </summary>
    public interface ITestClass : IXunitSerializable
    {
        /// <summary>
        /// Gets the class that this test case is attached to.
        /// </summary>
        ITypeInfo Class { get; }

        /// <summary>
        /// Gets the test collection this test case belongs to.
        /// </summary>
        ITestCollection TestCollection { get; }
    }
}