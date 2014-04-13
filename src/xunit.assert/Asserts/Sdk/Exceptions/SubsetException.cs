using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a set is not a subset of another set.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class SubsetException : AssertActualExpectedException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SubsetException"/> class.
        /// </summary>
        public SubsetException(IEnumerable expected, IEnumerable actual)
            : base(expected, actual, "Assert.Subset() Failure") { }
    }
}