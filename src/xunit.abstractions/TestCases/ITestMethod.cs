namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents a test method.
    /// </summary>
    public interface ITestMethod : IXunitSerializable
    {
        /// <summary>
        /// Gets the method associated with this test method.
        /// </summary>
        IMethodInfo Method { get; }

        /// <summary>
        /// Gets the test class that this test method belongs to.
        /// </summary>
        ITestClass TestClass { get; }
    }
}