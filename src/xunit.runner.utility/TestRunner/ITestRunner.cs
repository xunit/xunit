using System.Collections.Generic;

namespace Xunit
{
    /// <summary>
    /// Interface which represents a high level test runner.
    /// </summary>
    public interface ITestRunner
    {
        /// <summary>
        /// Executes the tests in the assembly.
        /// </summary>
        /// <returns>Returns true if there were no failures; return false otherwise.</returns>
        TestRunnerResult RunAssembly();

        /// <summary>
        /// Executes the tests in the assembly, and then executes the transforms with the
        /// resulting assembly XML.
        /// </summary>
        /// <param name="transforms">The transforms to execute.</param>
        /// <returns>Returns true if there were no failures; return false otherwise.</returns>
        TestRunnerResult RunAssembly(IEnumerable<IResultXmlTransform> transforms);

        /// <summary>
        /// Runs the class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        TestRunnerResult RunClass(string type);

        /// <summary>
        /// Runs a single test in a test class.
        /// </summary>
        /// <param name="type">The full name of the class.</param>
        /// <param name="method">The name of the method.</param>
        TestRunnerResult RunTest(string type, string method);

        /// <summary>
        /// Runs the list of tests in a test class.
        /// </summary>
        /// <param name="type">The full name of the class.</param>
        /// <param name="methods">The names of the methods to run.</param>
        TestRunnerResult RunTests(string type, List<string> methods);
    }
}