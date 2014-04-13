using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a set is not a proper superset of another set.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class ProperSupersetException : AssertActualExpectedException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ProperSupersetException"/> class.
        /// </summary>
        public ProperSupersetException(IEnumerable expected, IEnumerable actual)
            : base(expected, actual, "Assert.ProperSuperset() Failure") { }
    }
}