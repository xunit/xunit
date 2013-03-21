using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception to be thrown from theory execution when the number of
    /// parameter values does not the test method signature.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class ParameterCountMismatchException : Exception
    {
        /// <summary/>
        public ParameterCountMismatchException() { }

        /// <summary/>
        protected ParameterCountMismatchException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}