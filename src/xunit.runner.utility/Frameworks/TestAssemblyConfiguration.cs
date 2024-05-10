using System;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Represents the configuration items set in the App.config file of a test assembly.
    /// Should be read with the <see cref="ConfigReader"/> class.
    /// </summary>
    public class TestAssemblyConfiguration
    {
        /// <summary>
        /// Gets or sets a flag indicating whether an app domain should be used to discover and run tests.
        /// </summary>
        public AppDomainSupport? AppDomain { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether an app domain should be used to discover and run tests.
        /// If the flag is not set, returns the default value (<see cref="AppDomainSupport.IfAvailable"/>).
        /// </summary>
        public AppDomainSupport AppDomainOrDefault { get { return AppDomain ?? AppDomainSupport.IfAvailable; } }

        /// <summary>
        /// Gets or sets a flag indicating that the end user wants diagnostic messages
        /// from the test framework.
        /// </summary>
        public bool? DiagnosticMessages { get; set; }

        /// <summary>
        /// Gets a flag indicating that the end user wants diagnostic messages
        /// from the test framework. If the flag is not set, returns the default
        /// value (<c>false</c>).
        /// </summary>
        public bool DiagnosticMessagesOrDefault { get { return DiagnosticMessages ?? false; } }

        /// <summary>
        /// Gets or sets a flag indicating whether skipped tests should be turned into failures.
        /// </summary>
        public bool? FailSkips { get; set; }

        /// <summary>
        /// Gets a flag indicating whether skipped tests should be turned into failures. If the flag
        /// is not set, returns the default value (<c>false</c>).
        /// </summary>
        public bool FailSkipsOrDefault { get { return FailSkips ?? false; } }

        /// <summary>
        /// Gets or sets a flag indicating that the end user wants internal diagnostic messages
        /// from the test framework.
        /// </summary>
        public bool? InternalDiagnosticMessages { get; set; }

        /// <summary>
        /// Gets a flag indicating that the end user wants internal diagnostic messages
        /// from the test framework. If the flag is not set, returns the default
        /// value (<c>false</c>).
        /// </summary>
        public bool InternalDiagnosticMessagesOrDefault { get { return InternalDiagnosticMessages ?? false; } }

        /// <summary>
        /// Gets the number of seconds that a test can run before being considered "long running". Set to a positive
        /// value to enable the feature.
        /// </summary>
        public int? LongRunningTestSeconds { get; set; }

        /// <summary>
        /// Gets the number of seconds that a test can run before being considered "long running". If the value is not
        /// set, returns the default value (-1).
        /// </summary>
        public int LongRunningTestSecondsOrDefault { get { return LongRunningTestSeconds ?? -1; } }

        /// <summary>
        /// Gets or sets the maximum number of thread to use when parallelizing this assembly.
        /// </summary>
        public int? MaxParallelThreads { get; set; }

        /// <summary>
        /// Gets the maximum number of thread to use when parallelizing this assembly. If the value is not set (or set
        /// to 0), returns the default value (<see cref="Environment.ProcessorCount"/>).
        /// </summary>
        public int MaxParallelThreadsOrDefault
        {
            get
            {
                if (!MaxParallelThreads.HasValue || MaxParallelThreads == 0)
                    return Environment.ProcessorCount;

                return MaxParallelThreads.Value;
            }
        }

        /// <summary>
        /// Gets or sets the default display name for test methods.
        /// </summary>
        public TestMethodDisplay? MethodDisplay { get; set; }

        /// <summary>
        /// Gets the default display name for test methods. If the value is not set, returns
        /// the default value (<see cref="TestMethodDisplay.ClassAndMethod"/>).
        /// </summary>
        public TestMethodDisplay MethodDisplayOrDefault { get { return MethodDisplay ?? TestMethodDisplay.ClassAndMethod; } }

        /// <summary>
        /// Gets or sets the default display options for test methods.
        /// </summary>
        public TestMethodDisplayOptions? MethodDisplayOptions { get; set; }

        /// <summary>
        /// Gets the default display options for test methods. If the value is not set, returns
        /// the default value (<see cref="TestMethodDisplayOptions.None"/>).
        /// </summary>
        public TestMethodDisplayOptions MethodDisplayOptionsOrDefault { get { return MethodDisplayOptions ?? TestMethodDisplayOptions.None; } }

        /// <summary>
        /// Gets or sets the algorithm to be used for parallelization.
        /// </summary>
        public ParallelAlgorithm? ParallelAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets the algorithm to be used for parallelization.
        /// </summary>
        public ParallelAlgorithm ParallelAlgorithmOrDefault { get { return ParallelAlgorithm ?? Xunit.ParallelAlgorithm.Conservative; } }

        /// <summary>
        /// Gets or sets a flag indicating that this assembly is safe to parallelize against
        /// other assemblies.
        /// </summary>
        public bool? ParallelizeAssembly { get; set; }

        /// <summary>
        /// Gets a flag indicating that this assembly is safe to parallelize against
        /// other assemblies. If the flag is not set, returns the default value (<c>false</c>).
        /// </summary>
        public bool ParallelizeAssemblyOrDefault { get { return ParallelizeAssembly ?? false; } }

        /// <summary>
        /// Gets or sets a flag indicating that this test assembly wants to run test collections
        /// in parallel against one another.
        /// </summary>
        public bool? ParallelizeTestCollections { get; set; }

        /// <summary>
        /// Gets a flag indicating that this test assembly wants to run test collections
        /// in parallel against one another. If the flag is not set, returns the default
        /// value (<c>true</c>).
        /// </summary>
        public bool ParallelizeTestCollectionsOrDefault { get { return ParallelizeTestCollections ?? true; } }

        /// <summary>
        /// Gets or sets a flag indicating whether theory data should be pre-enumerated during
        /// test discovery.
        /// </summary>
        public bool? PreEnumerateTheories { get; set; }

        /// <summary>
        /// Gets a flag indicating whether theory data should be pre-enumerated during
        /// test discovery. If the flag is not set, returns the default value (<c>true</c>).
        /// </summary>
        public bool PreEnumerateTheoriesOrDefault { get { return PreEnumerateTheories ?? true; } }

        /// <summary>
        /// Gets or sets a flag indicating whether shadow copies should be used.
        /// </summary>
        public bool? ShadowCopy { get; set; }

        /// <summary>
        /// Gets a flag indicating whether shadow copies should be used. If the flag is not set,
        /// returns the default value (<c>true</c>).
        /// </summary>
        public bool ShadowCopyOrDefault { get { return ShadowCopy ?? true; } }

        /// <summary>
        /// Gets or sets a flag indicating whether output from <see cref="ITestOutputHelper"/> should be
        /// shown live as they're logged (in addition to being collected together after the test finishes).
        /// </summary>
        public bool? ShowLiveOutput { get; set; }

        /// <summary>
        /// Gets a flag indicating whether output from <see cref="ITestOutputHelper"/> should be
        /// shown live as they're logged (in addition to being collected together after the test finishes).
        /// If the flag is not set, returns the default value (<c>false</c>).
        /// </summary>
        public bool ShowLiveOutputOrDefault { get { return ShowLiveOutput ?? false; } }

        /// <summary>
        /// Gets or sets a flag indicating whether testing should stop on a failure.
        /// </summary>
        public bool? StopOnFail { get; set; }

        /// <summary>
        /// Gets a flag indicating whether testing should stop on a test failure. If the flag is not set,
        /// returns the default value (<c>false</c>).
        /// </summary>
        public bool StopOnFailOrDefault { get { return StopOnFail ?? false; } }
    }
}
