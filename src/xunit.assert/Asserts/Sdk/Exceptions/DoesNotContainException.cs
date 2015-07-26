namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a collection unexpectedly contains the expected value.
    /// </summary>
    public class DoesNotContainException : AssertActualExpectedException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DoesNotContainException"/> class.
        /// </summary>
        /// <param name="expected">The expected object value</param>
        /// <param name="actual">The actual value</param>
        public DoesNotContainException(object expected, object actual)
            : base(expected, actual, "Assert.DoesNotContain() Failure", "Found", "In value")
        { }
    }
}