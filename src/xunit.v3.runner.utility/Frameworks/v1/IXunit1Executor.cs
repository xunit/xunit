#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Xunit
{
    /// <summary>
    /// Represents a wrapper around the Executor class from xUnit.net v1.
    /// </summary>
    public interface IXunit1Executor : IDisposable
    {
        /// <summary>
        /// Gets the display name of the test framework.
        /// </summary>
        string TestFrameworkDisplayName { get; }

        /// <summary>
        /// Enumerates the tests in the assembly.
        /// </summary>
        /// <param name="handler">The callback handler used to return information.</param>
        void EnumerateTests(ICallbackEventHandler handler);

        /// <summary>
        /// Runs the tests in a class.
        /// </summary>
        /// <param name="type">The class to run.</param>
        /// <param name="methods">The methods in the class to run.</param>
        /// <param name="handler">The callback handler used to return information.</param>
        void RunTests(string type, List<string> methods, ICallbackEventHandler handler);
    }
}

#endif
