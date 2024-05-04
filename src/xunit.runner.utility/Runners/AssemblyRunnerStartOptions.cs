using System;

namespace Xunit.Runners
{
    /// <summary>
    /// Represents options to be used when calling
    /// <see cref="AssemblyRunner.Start(AssemblyRunnerStartOptions)"/>.
    /// </summary>
    public class AssemblyRunnerStartOptions
    {
        string[] typesToRun = [];

        /// <summary>
        /// Indicates whether diagnostic messages should be generated. If unset (or set
        /// to <c>null</c>), will use the value from the configuration file (and if that
        /// isn't set, will use the default value of <c>false</c>).
        /// </summary>
        public bool? DiagnosticMessages { get; set; }

        /// <summary>
        /// Gets an empty set of options (representing all default behavior).
        /// </summary>
        public static AssemblyRunnerStartOptions Empty => new();

        /// <summary>
        /// Indicates whether internal diagnostic messages should be generated (these are
        /// typically low level diagnostic messages from the test engine itself that may
        /// be requested by xUnit.net developers when debugging issues inside xUnit.net
        /// itself). If unset (or set to <c>null</c>), will use the value from the
        /// configuraiton file (and if that isn't set, will use the default value
        /// of <c>false</c>).
        /// </summary>
        public bool? InternalDiagnosticMessages { get; set; }

        /// <summary>
        /// Indicates how many threads to use to run parallel tests (will have no affect
        /// if parallelism is turned off). A value of <c>-1</c> indicates a desire for
        /// no thread limit; a value of <c>0</c> indicates a desire for the default
        /// limit (which is <see cref="Environment.ProcessorCount"/>); a value greater
        /// than 0 indicates an exact thread count is desired. If unset (or set to
        /// <c>null</c>), will use the value from the configuration file (and if that
        /// isn't set, will use the default value of <see cref="Environment.ProcessorCount"/>).
        /// </summary>
        public int? MaxParallelThreads { get; set; }

        /// <summary>
        /// Indicates how to display test methods. If unset (or set to <c>null</c>),
        /// will use the value from the configuration file (and if that isn't set,
        /// will use the default value of <see cref="TestMethodDisplay.ClassAndMethod"/>).
        /// </summary>
        public TestMethodDisplay? MethodDisplay { get; set; }

        /// <summary>
        /// Indicates how to interpret test method names for display. If unset (or set
        /// to <c>null</c>), will use the value from the configuration file (and if that
        /// isn't set, will use the default value of <see cref="TestMethodDisplayOptions.None"/>).
        /// </summary>
        public TestMethodDisplayOptions? MethodDisplayOptions { get; set; }

        /// <summary>
        /// Indicates whether to run test collections in parallel. If unset (or set to
        /// <c>null</c>), will use the value from the configuration file (and if that
        /// isn't set, will use the default value of <c>true</c>). Note that test
        /// collection parallelization is only available in v2 test projects.
        /// </summary>
        public bool? Parallel { get; set; }

        /// <summary>
        /// Indicates which algorithm to use when parallelizing tests (will have no effect
        /// if parallelism is turned off or if the max parallel threads is set to <c>-1</c>).
        /// If unset (or set to <c>null</c>), will use the value from the configuration
        /// file (and if that isn't set, will use the default value of
        /// <see cref="ParallelAlgorithm.Conservative"/>. For more information on the
        /// parallelism algorithms, see
        /// <see href="https://xunit.net/docs/running-tests-in-parallel#algorithms"/>.
        /// </summary>
        public ParallelAlgorithm? ParallelAlgorithm { get; set; }

        /// <summary>
        /// Indicates whether theories should be pre-enumerated (that is, enumerated during
        /// discovery rather than during execution). If unset (or set to <c>null</c>),
        /// will use the value from the configuration file (and if that isn't set,
        /// will use the default value of <c>false</c>).
        /// </summary>
        public bool? PreEnumerateTheories { get; set; }

        /// <summary>
        /// Indicates the types to be run. If empty, will run all types in the assembly.
        /// </summary>
        public string[] TypesToRun
        {
            get => typesToRun;
            set => typesToRun = value ?? [];
        }
    }
}
