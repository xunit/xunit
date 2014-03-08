using System;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception to be thrown from theory execution when the number of
    /// parameter values does not the test method signature.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class ParameterCountMismatchException : Exception
    {
        /// <summary/>
        public ParameterCountMismatchException() { }
    }
}