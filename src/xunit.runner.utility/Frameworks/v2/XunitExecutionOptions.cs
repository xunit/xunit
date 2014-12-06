namespace Xunit
{
    /// <summary>
    /// Represents execution options for xUnit.net v2 tests.
    /// </summary>
    public class XunitExecutionOptions : TestFrameworkOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XunitExecutionOptions"/> class.
        /// </summary>
        /// <param name="configuration">The optional configuration to copy values from.</param>
        public XunitExecutionOptions(TestAssemblyConfiguration configuration = null)
        {
            if (configuration != null)
            {
                DiagnosticMessages = configuration.DiagnosticMessages;
                DisableParallelization = !configuration.ParallelizeTestCollections;
                MaxParallelThreads = configuration.MaxParallelThreads;
            }
        }

        /// <summary>
        /// Gets or sets a flag that determines whether diagnostic messages will be emitted.
        /// </summary>
        public bool DiagnosticMessages
        {
            get { return GetValue<bool>(TestOptionsNames.Execution.DiagnosticMessages, false); }
            set { SetValue(TestOptionsNames.Execution.DiagnosticMessages, value); }
        }

        /// <summary>
        /// Gets or sets a flag to disable parallelization.
        /// </summary>
        public bool DisableParallelization
        {
            get { return GetValue<bool>(TestOptionsNames.Execution.DisableParallelization, false); }
            set { SetValue(TestOptionsNames.Execution.DisableParallelization, value); }
        }

        /// <summary>
        /// Gets or sets the maximum number of threads to use when running tests in parallel.
        /// If set to 0 (the default value), does not limit the number of threads.
        /// </summary>
        public int MaxParallelThreads
        {
            get { return GetValue<int>(TestOptionsNames.Execution.MaxParallelThreads, 0); }
            set { SetValue(TestOptionsNames.Execution.MaxParallelThreads, value); }
        }

        /// <summary>
        /// Gets or sets a flag that determines whether xUnit.net should report test results synchronously.
        /// </summary>
        public bool SynchronousMessageReporting
        {
            get { return GetValue<bool>(TestOptionsNames.Execution.SynchronousMessageReporting, false); }
            set { SetValue(TestOptionsNames.Execution.SynchronousMessageReporting, value); }
        }
    }
}
