using System.Collections;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a set is not a proper subset of another set.
    /// </summary>
    public class ProperSubsetException : AssertActualExpectedException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ProperSubsetException"/> class.
        /// </summary>
        public ProperSubsetException(IEnumerable expected, IEnumerable actual)
            : base(expected, actual, "Assert.ProperSubset() Failure")
        { }
    }
}