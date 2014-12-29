using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Represents options passed to a test framework for discovery or execution.
    /// </summary>
    public class TestFrameworkOptions : LongLivedMarshalByRefObject, ITestFrameworkDiscoveryOptions, ITestFrameworkExecutionOptions
    {
        readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        // Force users to use one of the factory methods
        private TestFrameworkOptions() { }

        /// <summary>
        /// Creates an instance of <see cref="TestFrameworkOptions"/>
        /// </summary>
        /// <param name="configuration">The optional configuration to copy values from.</param>
        public static ITestFrameworkDiscoveryOptions ForDiscovery(TestAssemblyConfiguration configuration = null)
        {
            ITestFrameworkDiscoveryOptions result = new TestFrameworkOptions();

            if (configuration != null)
            {
                result.SetDiagnosticMessages(configuration.DiagnosticMessages);
                result.SetMethodDisplay(configuration.MethodDisplay);
                result.SetPreEnumerateTheories(configuration.PreEnumerateTheories);
            }

            return result;
        }

        /// <summary>
        /// Creates an instance of <see cref="TestFrameworkOptions"/>
        /// </summary>
        /// <param name="configuration">The optional configuration to copy values from.</param>
        public static ITestFrameworkExecutionOptions ForExecution(TestAssemblyConfiguration configuration = null)
        {
            ITestFrameworkExecutionOptions result = new TestFrameworkOptions();

            if (configuration != null)
            {
                result.SetDiagnosticMessages(configuration.DiagnosticMessages);
                result.SetDisableParallelization(!configuration.ParallelizeTestCollections);
                result.SetMaxParallelThreads(configuration.MaxParallelThreads);
            }

            return result;
        }

        /// <summary>
        /// Gets a value from the options collection.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <param name="defaultValue">The default value to use if the value is not present.</param>
        /// <returns>Returns the value.</returns>
        public TValue GetValue<TValue>(string name, TValue defaultValue)
        {
            object result;
            if (properties.TryGetValue(name, out result))
                return (TValue)result;

            return defaultValue;
        }

        /// <summary>
        /// Sets a value into the options collection.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The value.</param>
        public void SetValue<TValue>(string name, TValue value)
        {
            properties[name] = value;
        }
    }
}
