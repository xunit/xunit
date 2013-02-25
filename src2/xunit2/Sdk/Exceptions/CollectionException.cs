using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when Assert.Collection fails.
    /// </summary>
    [Serializable]
    public class CollectionException : AssertException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CollectionException"/> class.
        /// </summary>
        /// <param name="expectedCount">The expected number of items in the collection.</param>
        /// <param name="actualCount">The actual number of items in the collection.</param>
        /// <param name="indexFailurePoint">The index of the position where the first comparison failure occurred.</param>
        /// <param name="innerException">The exception that was thrown during the comparison failure.</param>
        public CollectionException(int expectedCount, int actualCount, int indexFailurePoint = -1, Exception innerException = null)
            : base("Assert.Collection() Failure", innerException)
        {
            ExpectedCount = expectedCount;
            ActualCount = actualCount;
            IndexFailurePoint = indexFailurePoint;
        }

        /// <inheritdoc/>
        protected CollectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ActualCount = info.GetInt32("ActualCount");
            ExpectedCount = info.GetInt32("ExpectedCount");
            IndexFailurePoint = info.GetInt32("IndexFailurePoint");
        }

        /// <summary>
        /// The actual number of items in the collection.
        /// </summary>
        public int ActualCount { get; set; }

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
                    return String.Format(CultureInfo.CurrentCulture,
                                         "{0}{3}Error during comparison of item at index {1}{3}Inner exception: {2}",
                                         base.Message,
                                         IndexFailurePoint,
                                         FormatInnerException(),
                                         Environment.NewLine);

                return String.Format(CultureInfo.CurrentCulture,
                                     "{0}{3}Expected item count: {1}{3}Actual item count:   {2}",
                                     base.Message,
                                     ExpectedCount,
                                     ActualCount,
                                     Environment.NewLine);
            }
        }

        public override string StackTrace
        {
            get
            {
                if (InnerException == null)
                    return base.StackTrace;

                return InnerException.StackTrace + Environment.NewLine + base.StackTrace;
            }
        }

        private string FormatInnerException()
        {
            var lines = InnerException.Message
                                      .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select((value, idx) => idx > 0 ? "        " + value : value);

            return String.Join(Environment.NewLine, lines);
        }

        /// <inheritdoc/>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Guard.ArgumentNotNull("info", info);

            info.AddValue("ActualCount", ActualCount);
            info.AddValue("ExpectedCount", ExpectedCount);
            info.AddValue("IndexFailurePoint", IndexFailurePoint);

            base.GetObjectData(info, context);
        }
    }
}
