using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when two values are unexpectedly not equal.
    /// </summary>
    public class EqualException : AssertActualExpectedException
    {
        static readonly Dictionary<char, string> Encodings = new Dictionary<char, string>
        {
            { '\r', "\\r" },
            { '\n', "\\n" },
            { '\t', "\\t" },
            { '\0', "\\0" }
        };

        string message;

        /// <summary>
        /// Creates a new instance of the <see cref="EqualException"/> class.
        /// </summary>
        /// <param name="expected">The expected object value</param>
        /// <param name="actual">The actual object value</param>
        public EqualException(object expected, object actual)
            : base(expected, actual, "Assert.Equal() Failure")
        {
            ActualIndex = -1;
            ExpectedIndex = -1;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="EqualException"/> class for string comparisons.
        /// </summary>
        /// <param name="expected">The expected string value</param>
        /// <param name="actual">The actual string value</param>
        /// <param name="expectedIndex">The first index in the expected string where the strings differ</param>
        /// <param name="actualIndex">The first index in the actual string where the strings differ</param>
        public EqualException(string expected, string actual, int expectedIndex, int actualIndex)
            : base(expected, actual, "Assert.Equal() Failure")
        {
            ActualIndex = actualIndex;
            ExpectedIndex = expectedIndex;
        }

        /// <summary>
        /// Gets the index into the actual value where the values first differed.
        /// Returns -1 if the difference index points were not provided.
        /// </summary>
        public int ActualIndex { get; private set; }

        /// <summary>
        /// Gets the index into the expected value where the values first differed.
        /// Returns -1 if the difference index points were not provided.
        /// </summary>
        public int ExpectedIndex { get; private set; }

        /// <inheritdoc/>
        public override string Message
        {
            get
            {
                if (message == null)
                    message = CreateMessage();

                return message;
            }
        }

        string CreateMessage()
        {
            if (ExpectedIndex == -1)
                return base.Message;

            Tuple<string, string> printedExpected = ShortenAndEncode(Expected, ExpectedIndex, '↓');
            Tuple<string, string> printedActual = ShortenAndEncode(Actual, ActualIndex, '↑');

            return string.Format(
                CultureInfo.CurrentCulture,
                "{1}{0}          {2}{0}Expected: {3}{0}Actual:   {4}{0}          {5}",
                Environment.NewLine,
                UserMessage,
                printedExpected.Item2,
                printedExpected.Item1 ?? "(null)",
                printedActual.Item1 ?? "(null)",
                printedActual.Item2
            );
        }

        static Tuple<string, string> ShortenAndEncode(string value, int position, char pointer)
        {
            int start = Math.Max(position - 20, 0);
            int end = Math.Min(position + 41, value.Length);
            var printedValue = new StringBuilder(100);
            var printedPointer = new StringBuilder(100);

            if (start > 0)
            {
                printedValue.Append("···");
                printedPointer.Append("   ");
            }

            for (int idx = start; idx < end; ++idx)
            {
                char c = value[idx];
                string encoding;
                int paddingLength = 1;

                if (Encodings.TryGetValue(c, out encoding))
                {
                    printedValue.Append(encoding);
                    paddingLength = encoding.Length;
                }
                else
                    printedValue.Append(c);

                if (idx < position)
                    printedPointer.Append(' ', paddingLength);
                else if (idx == position)
                    printedPointer.AppendFormat("{0} (pos {1})", pointer, position);
            }

            if (value.Length == position)
                printedPointer.AppendFormat("{0} (pos {1})", pointer, position);

            if (end < value.Length)
                printedValue.Append("···");

            return new Tuple<string, string>(printedValue.ToString(), printedPointer.ToString());
        }
    }
}