using System;
using System.Collections.Generic;
using System.Xml;

namespace Xunit
{
    /// <summary>
    /// Wraps calls to the Executor. Used by runners to perform version-resilient test
    /// enumeration and execution.
    /// </summary>
    public interface IExecutorWrapper : IDisposable
    {
        /// <summary>
        /// Gets the full pathname to the assembly under test.
        /// </summary>
        string AssemblyFilename { get; }

        /// <summary>
        /// Gets the full pathname to the configuration file.
        /// </summary>
        string ConfigFilename { get; }

        /// <summary>
        /// Gets the version of xunit.dll used by the test assembly.
        /// </summary>
        string XunitVersion { get; }

        /// <summary>
        /// Enumerates the tests in an assembly.
        /// </summary>
        /// <returns>The fully-formed assembly node of the XML</returns>
        XmlNode EnumerateTests();

        /// <summary>
        /// Gets a count of the tests in the assembly.
        /// </summary>
        /// <returns>Returns the number of tests, if known; returns -1 if not known. May not represent
        /// an exact count, but should be a best effort guess by the framework.</returns>
        int GetAssemblyTestCount();

        /// <summary>
        /// Runs all the tests in an assembly.
        /// </summary>
        /// <param name="callback">The callback which is called as each test/class/assembly is
        /// finished, providing XML nodes that are part of the xUnit.net XML output format.
        /// Test runs can be cancelled by returning false to the callback. If null, there are
        /// no status callbacks (and cancellation isn't possible).</param>
        /// <returns>Returns the fully-formed assembly node for the assembly that was just run.</returns>
        XmlNode RunAssembly(Predicate<XmlNode> callback);

        /// <summary>
        /// Runs all the tests in the given class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="callback">The callback which is called as each test/class is
        /// finished, providing XML nodes that are part of the xUnit.net XML output format.
        /// Test runs can be cancelled by returning false to the callback. If null, there are
        /// no status callbacks (and cancellation isn't possible).</param>
        /// <returns>Returns the fully-formed class node for the class that was just run.</returns>
        XmlNode RunClass(string type, Predicate<XmlNode> callback);

        /// <summary>
        /// Runs a single test in a class.
        /// </summary>
        /// <param name="type">The type to run.</param>
        /// <param name="method">The method to run.</param>
        /// <param name="callback">The callback which is called as each test/class is
        /// finished, providing XML nodes that are part of the xUnit.net XML output format.
        /// Test runs can be cancelled by returning false to the callback. If null, there are
        /// no status callbacks (and cancellation isn't possible).</param>
        /// <returns>Returns the fully-formed class node for the class of the test that was just run.</returns>
        XmlNode RunTest(string type, string method, Predicate<XmlNode> callback);

        /// <summary>
        /// Runs several tests in a single class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="methods">The methods to run.</param>
        /// <param name="callback">The callback which is called as each test/class is
        /// finished, providing XML nodes that are part of the xUnit.net XML output format.
        /// Test runs can be cancelled by returning false to the callback. If null, there are
        /// no status callbacks (and cancellation isn't possible).</param>
        /// <returns>Returns the fully-formed class node for the class of the tests that were just run.</returns>
        XmlNode RunTests(string type, List<string> methods, Predicate<XmlNode> callback);
    }
}