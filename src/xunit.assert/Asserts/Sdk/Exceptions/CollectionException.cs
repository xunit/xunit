using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when Assert.Collection fails.
    /// </summary>
    public class CollectionException : XunitException
    {
        readonly string innerException;
        readonly string innerStackTrace;

        readonly IEnumerable actualElements;

        /// <summary>
        /// Creates a new instance of the <see cref="CollectionException"/> class.
        /// </summary>
        /// <param name="expectedCount">The expected number of items in the collection.</param>
        /// <param name="actualCount">The actual number of items in the collection.</param>
        /// <param name="indexFailurePoint">The index of the position where the first comparison failure occurred.</param>
        /// <param name="innerException">The exception that was thrown during the comparison failure.</param>
        /// <param name="actualElements">The elements of the collection</param>
        public CollectionException(int expectedCount, int actualCount, int indexFailurePoint = -1, Exception innerException = null, IEnumerable actualElements = null)
            : base("Assert.Collection() Failure")
        {
            ExpectedCount = expectedCount;
            ActualCount = actualCount;
            IndexFailurePoint = indexFailurePoint;
            this.innerException = FormatInnerException(innerException);
            innerStackTrace = innerException == null ? null : innerException.StackTrace;

            this.actualElements = actualElements;
        }

        /// <summary>
        /// The actual number of items in the collection.
        /// </summary>
        public int ActualCount { get; set; }

        /// <summary>
        /// The list of actual Elements, optional and null by default
        /// </summary>
        public IEnumerable ActualElements
        {
            get { return this.actualElements; }
        }

        /// <summary>
        /// The expected number of items in the collection.
        /// </summary>
        public int ExpectedCount { get; set; }

        /// <summary>
        /// The index of the position where the first comparison failure occurred, or -1 if
        /// comparisions did not occur (because the actual and expected counts differed).
        /// </summary>
        public int IndexFailurePoint { get; set; }

        /// <inheritdoc/>
        public override string Message
        {
            get
            {
                if (IndexFailurePoint >= 0)
                    return string.Format(CultureInfo.CurrentCulture,
                                         "{0}{3}Error during comparison of item at index {1}{3}Inner exception: {2}",
                                         base.Message,
                                         IndexFailurePoint,
                                         innerException,
                                         Environment.NewLine);

                return string.Format(CultureInfo.CurrentCulture,
                                     "{0}{3}Expected item count: {1}{3}Actual item count:   {2}",
                                     base.Message,
                                     ExpectedCount,
                                     ActualCount,
                                     Environment.NewLine);
            }
        }

        /// <inheritdoc/>
        public override string StackTrace
        {
            get
            {
                if (innerStackTrace == null)
                    return base.StackTrace;

                return innerStackTrace + Environment.NewLine + base.StackTrace;
            }
        }

        static string FormatInnerException(Exception innerException)
        {
            if (innerException == null)
                return null;

            var lines = innerException.Message
                                      .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select((value, idx) => idx > 0 ? "        " + value : value);

            return string.Join(Environment.NewLine, lines);
        }
    }
}
