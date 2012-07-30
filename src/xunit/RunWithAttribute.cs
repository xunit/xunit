using System;
using System.Diagnostics.CodeAnalysis;

namespace Xunit
{
    /// <summary>
    /// Attributes used to decorate a test fixture that is run with an alternate test runner.
    /// The test runner must implement the <see cref="Xunit.Sdk.ITestClassCommand"/> interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is designed as an extensibility point.")]
    public class RunWithAttribute : Attribute
    {
        /// <summary>
        /// Creates a new instance of the <see cref="RunWithAttribute"/> class.
        /// </summary>
        /// <param name="testClassCommand">The class which implements ITestClassCommand and acts as the runner
        /// for the test fixture.</param>
        public RunWithAttribute(Type testClassCommand)
        {
            TestClassCommand = testClassCommand;
        }

        /// <summary>
        /// Gets the test class command.
        /// </summary>
        public Type TestClassCommand { get; private set; }
    }
}