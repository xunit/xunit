using System;
using System.Collections.Generic;

namespace Xunit.Sdk
{
    /// <summary>
    /// Interface which describes the ability to executes all the tests in a test class.
    /// </summary>
    public interface ITestClassCommand
    {
        /// <summary>
        /// Gets the object instance that is under test. May return null if you wish
        /// the test framework to create a new object instance for each test method.
        /// </summary>
        object ObjectUnderTest { get; }

        /// <summary>
        /// Gets or sets the type that is being tested
        /// </summary>
        ITypeInfo TypeUnderTest { get; set; }

        /// <summary>
        /// Allows the test class command to choose the next test to be run from the list of
        /// tests that have not yet been run, thereby allowing it to choose the run order.
        /// </summary>
        /// <param name="testsLeftToRun">The tests remaining to be run</param>
        /// <returns>The index of the test that should be run</returns>
        int ChooseNextTest(ICollection<IMethodInfo> testsLeftToRun);

        /// <summary>
        /// Execute actions to be run after all the test methods of this test class are run.
        /// </summary>
        /// <returns>Returns the <see cref="Exception"/> thrown during execution, if any; null, otherwise</returns>
        Exception ClassFinish();

        /// <summary>
        /// Execute actions to be run before any of the test methods of this test class are run.
        /// </summary>
        /// <returns>Returns the <see cref="Exception"/> thrown during execution, if any; null, otherwise</returns>
        Exception ClassStart();

        /// <summary>
        /// Enumerates the test commands for a given test method in this test class.
        /// </summary>
        /// <param name="testMethod">The method under test</param>
        /// <returns>The test commands for the given test method</returns>
        IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod);

        /// <summary>
        /// Enumerates the methods which are test methods in this test class.
        /// </summary>
        /// <returns>The test methods</returns>
        IEnumerable<IMethodInfo> EnumerateTestMethods();

        /// <summary>
        /// Determines if a given <see cref="IMethodInfo"/> refers to a test method.
        /// </summary>
        /// <param name="testMethod">The test method to validate</param>
        /// <returns>True if the method is a test method; false, otherwise</returns>
        bool IsTestMethod(IMethodInfo testMethod);
    }
}