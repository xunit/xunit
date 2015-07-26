using System;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception to be thrown from theory execution when the number of
    /// parameter values does not the test method signature.
    /// </summary>
    public class ParameterCountMismatchException : Exception { }
}
