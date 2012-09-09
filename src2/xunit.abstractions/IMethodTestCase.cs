namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents a test case which is associated with a method.
    /// </summary>
    public interface IMethodTestCase : IClassTestCase
    {
        /// <summary>
        /// Gets the method associated with this test case.
        /// </summary>
        IMethodInfo Method { get; }
    }
}
