namespace Xunit.Sdk
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Exception thrown when code unexpectedly fails to raise an event.
    /// </summary>
    public class RaisesException : XunitException
    {
        readonly string stackTrace = null;

        /// <summary>
        /// Creates a new instance of the <see cref="RaisesException" /> class. Call this constructor
        /// when no event was raised.
        /// </summary>
        /// <param name="expected">The type of the event args that was expected</param>
        public RaisesException(Type expected)
            : base("(No event was raised)")
        {
            Expected = ConvertToSimpleTypeName(expected.GetTypeInfo());
            Actual = base.UserMessage;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RaisesException" /> class. Call this constructor
        /// when an
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public RaisesException(Type expected, Type actual)
            : base("(Raised event did not match expected event)")
        {
            Expected = ConvertToSimpleTypeName(expected.GetTypeInfo());
            Actual = ConvertToSimpleTypeName(actual.GetTypeInfo());
        }

        /// <summary>
        /// Gets the actual value.
        /// </summary>
        public string Actual { get; }

        /// <summary>
        /// Gets the expected value.
        /// </summary>
        public string Expected { get; }

        /// <summary>
        /// Gets a message that describes the current exception. Includes the expected and actual values.
        /// </summary>
        /// <returns>The error message that explains the reason for the exception, or an empty string("").</returns>
        /// <filterpriority>1</filterpriority>
        public override string Message
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture,
                                     "{0}{3}{1}{3}{2}",
                                     base.Message,
                                     Expected ?? "(null)",
                                     Actual ?? "(null)",
                                     Environment.NewLine);
            }
        }

        /// <summary>
        /// Gets a string representation of the frames on the call stack at the time the current exception was thrown.
        /// </summary>
        /// <returns>A string that describes the contents of the call stack, with the most recent method call appearing first.</returns>
        public override string StackTrace
        {
            get { return stackTrace ?? base.StackTrace; }
        }

        static string ConvertToSimpleTypeName(TypeInfo typeInfo)
        {
            if (!typeInfo.IsGenericType)
                return typeInfo.Name;

            var simpleNames = typeInfo.GenericTypeArguments.Select(type => ConvertToSimpleTypeName(type.GetTypeInfo()));
            var backTickIdx = typeInfo.Name.IndexOf('`');
            if (backTickIdx < 0)
                backTickIdx = typeInfo.Name.Length;  // F# doesn't use backticks for generic type names

            return string.Format("{0}<{1}>", typeInfo.Name.Substring(0, backTickIdx), string.Join(", ", simpleNames));
        }
    }
}
