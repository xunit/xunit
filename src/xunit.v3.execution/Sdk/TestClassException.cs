using System;

namespace Xunit.Sdk
{
    /// <summary>
    /// Represents an exception that happened during the process of a test class. This typically
    /// means there were problems identifying the correct test class constructor, or problems
    /// creating the fixture data for the test class.
    /// </summary>
#if NETFRAMEWORK
    [Serializable]
#endif
    public class TestClassException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestClassException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public TestClassException(string message)
            : base(message)
        { }

#if NETFRAMEWORK
        /// <inheritdoc/>
        protected TestClassException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }
#endif
    }
}
