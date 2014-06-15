namespace Xunit
{
    /// <summary>
    /// Represents execution options for xUnit.net v2 tests.
    /// </summary>
    public class XunitExecutionOptions : TestFrameworkOptions
    {
        /// <summary>
        /// Gets or sets a flag that determines whether xUnit.net should report test results synchronously.
        /// </summary>
        public bool SynchronousMessageReporting
        {
            get { return GetValue<bool>(TestOptionsNames.Execution.SynchronousMessageReporting, false); }
            set { SetValue(TestOptionsNames.Execution.SynchronousMessageReporting, value); }
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
    }
}
