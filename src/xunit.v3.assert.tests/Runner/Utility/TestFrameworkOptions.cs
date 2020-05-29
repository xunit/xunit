using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// TODO: WE COPIED THIS FROM RUNNER UTILITY, WHERE DOES IT REALLY BELONG?
    /// </summary>
    [DebuggerDisplay("{ToDebuggerDisplay(),nq}")]
#if NETFRAMEWORK
    [System.Serializable]
#endif
    public class TestFrameworkOptions : ITestFrameworkDiscoveryOptions, ITestFrameworkExecutionOptions
    {
        readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        // Force users to use one of the factory methods
        TestFrameworkOptions() { }

        /// <summary>
        /// Creates an instance of <see cref="TestFrameworkOptions"/>
        /// </summary>
        /// <param name="configuration">The optional configuration to copy values from.</param>
        [SuppressMessage("Language Usage Opportunities", "RECS0091:Use 'var' keyword when possible", Justification = "Using var here causes ambiguity with the SetDiagnosticMessages extension method")]
        public static ITestFrameworkDiscoveryOptions ForDiscovery(/*TestAssemblyConfiguration configuration = null*/)
        {
            ITestFrameworkDiscoveryOptions result = new TestFrameworkOptions();

            //if (configuration != null)
            //{
            //    result.SetDiagnosticMessages(configuration.DiagnosticMessages);
            //    result.SetInternalDiagnosticMessages(configuration.InternalDiagnosticMessages);
            //    result.SetMethodDisplay(configuration.MethodDisplay);
            //    result.SetMethodDisplayOptions(configuration.MethodDisplayOptions);
            //    result.SetPreEnumerateTheories(configuration.PreEnumerateTheories);
            //}

            return result;
        }

        /// <summary>
        /// Creates an instance of <see cref="TestFrameworkOptions"/>
        /// </summary>
        /// <param name="configuration">The optional configuration to copy values from.</param>
        [SuppressMessage("Language Usage Opportunities", "RECS0091:Use 'var' keyword when possible", Justification = "Using var here causes ambiguity with the SetDiagnosticMessages extension method")]
        public static ITestFrameworkExecutionOptions ForExecution(/*TestAssemblyConfiguration configuration = null*/)
        {
            ITestFrameworkExecutionOptions result = new TestFrameworkOptions();

            //if (configuration != null)
            //{
            //    result.SetDiagnosticMessages(configuration.DiagnosticMessages);
            //    result.SetInternalDiagnosticMessages(configuration.InternalDiagnosticMessages);
            //    result.SetDisableParallelization(!configuration.ParallelizeTestCollections);
            //    result.SetMaxParallelThreads(configuration.MaxParallelThreads);
            //}

            return result;
        }

        /// <summary>
        /// Gets a value from the options collection.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <returns>Returns the value.</returns>
        public TValue GetValue<TValue>(string name)
        {
            object result;
            if (properties.TryGetValue(name, out result))
                return (TValue)result;

            return default(TValue);
        }

        /// <summary>
        /// Sets a value into the options collection.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The value.</param>
        public void SetValue<TValue>(string name, TValue value)
        {
            if (value == null)
                properties.Remove(name);
            else
                properties[name] = value;
        }

        string ToDebuggerDisplay()
            => $"{{ {string.Join(", ", properties.Select(p => string.Format("{{ {0} = {1} }}", new object[] { p.Key, ToDebuggerDisplay(p.Value) })).ToArray())} }}";

        string ToDebuggerDisplay(object value)
        {
            if (value == null)
                return "null";

            if (value is string stringValue)
                return $"\"{stringValue}\"";

            return value.ToString();
        }
    }
}
