using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Base class for exceptions that have actual and expected values
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class AssertActualExpectedException : AssertException
    {
        readonly string actual;
        readonly string differencePosition = "";
        readonly string expected;

        /// <summary>
        /// Creates a new instance of the <see href="AssertActualExpectedException"/> class.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="userMessage">The user message to be shown</param>
        public AssertActualExpectedException(object expected, object actual, string userMessage)
            : this(expected, actual, userMessage, false) { }

        /// <summary>
        /// Creates a new instance of the <see href="AssertActualExpectedException"/> class.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="userMessage">The user message to be shown</param>
        /// <param name="skipPositionCheck">Set to true to skip the check for difference position</param>
        public AssertActualExpectedException(object expected, object actual, string userMessage, bool skipPositionCheck)
            : base(userMessage)
        {
            if (!skipPositionCheck)
            {
                IEnumerable enumerableActual = actual as IEnumerable;
                IEnumerable enumerableExpected = expected as IEnumerable;

                if (enumerableActual != null && enumerableExpected != null)
                {
                    IEnumerator enumeratorActual = enumerableActual.GetEnumerator();
                    IEnumerator enumeratorExpected = enumerableExpected.GetEnumerator();
                    int position = 0;

                    while (true)
                    {
                        bool actualHasNext = enumeratorActual.MoveNext();
                        bool expectedHasNext = enumeratorExpected.MoveNext();

                        if (!actualHasNext || !expectedHasNext)
                            break;

                        if (!Equals(enumeratorActual.Current, enumeratorExpected.Current))
                            break;

                        position++;
                    }

                    differencePosition = "Position: First difference is at position " + position + Environment.NewLine;
                }
            }

            this.actual = actual == null ? null : ConvertToString(actual);
            this.expected = expected == null ? null : ConvertToString(expected);

            if (actual != null &&
                expected != null &&
                actual.ToString() == expected.ToString() &&
                actual.GetType() != expected.GetType())
            {
                this.actual += String.Format(" ({0})", actual.GetType().FullName);
                this.expected += String.Format(" ({0})", expected.GetType().FullName);
            }
        }

        /// <inheritdoc/>
        protected AssertActualExpectedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            actual = info.GetString("Actual");
            differencePosition = info.GetString("DifferencePosition");
            expected = info.GetString("Expected");
        }

        /// <summary>
        /// Gets the actual value.
        /// </summary>
        public string Actual
        {
            get { return actual; }
        }

        /// <summary>
        /// Gets the expected value.
        /// </summary>
        public string Expected
        {
            get { return expected; }
        }

        /// <summary>
        /// Gets a message that describes the current exception. Includes the expected and actual values.
        /// </summary>
        /// <returns>The error message that explains the reason for the exception, or an empty string("").</returns>
        /// <filterpriority>1</filterpriority>
        public override string Message
        {
            get
            {
                return string.Format("{0}{4}{1}Expected: {2}{4}Actual:   {3}",
                                     base.Message,
                                     differencePosition,
                                     FormatMultiLine(Expected ?? "(null)"),
                                     FormatMultiLine(Actual ?? "(null)"),
                                     Environment.NewLine);
            }
        }

        private static string ConvertToSimpleTypeName(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            Type[] genericTypes = type.GetGenericArguments();
            string[] simpleNames = new string[genericTypes.Length];

            for (int idx = 0; idx < genericTypes.Length; idx++)
                simpleNames[idx] = ConvertToSimpleTypeName(genericTypes[idx]);

            string baseTypeName = type.Name;
            int backTickIdx = type.Name.IndexOf('`');

            return baseTypeName.Substring(0, backTickIdx) + "<" + String.Join(", ", simpleNames) + ">";
        }

        static string ConvertToString(object value)
        {
            string stringValue = value as string;
            if (stringValue != null)
                return stringValue;

            IEnumerable enumerableValue = value as IEnumerable;
            if (enumerableValue == null)
                return value.ToString();

            List<string> valueStrings = new List<string>();

            foreach (object valueObject in enumerableValue)
            {
                string displayName;

                if (valueObject == null)
                    displayName = "(null)";
                else
                {
                    string stringValueObject = valueObject as string;
                    if (stringValueObject != null)
                        displayName = "\"" + stringValueObject + "\"";
                    else
                        displayName = valueObject.ToString();
                }

                valueStrings.Add(displayName);
            }

            return ConvertToSimpleTypeName(value.GetType()) + " { " + String.Join(", ", valueStrings.ToArray()) + " }";
        }

        static string FormatMultiLine(string value)
        {
            return value.Replace(Environment.NewLine, Environment.NewLine + "          ");
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Protected with the Guard class")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Guard.ArgumentNotNull("info", info);

            info.AddValue("Actual", actual);
            info.AddValue("DifferencePosition", differencePosition);
            info.AddValue("Expected", expected);

            base.GetObjectData(info, context);
        }
    }
}