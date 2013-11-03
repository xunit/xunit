using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Represents an exception that happened during the process of a test class. This typically
    /// means there were problems identifying the correct test class constructor, or problems
    /// creating the fixture data for the test class.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class TestClassException : XunitException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestClassException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public TestClassException(string message) : base(message) { }

        /// <inheritdoc/>
        protected TestClassException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
