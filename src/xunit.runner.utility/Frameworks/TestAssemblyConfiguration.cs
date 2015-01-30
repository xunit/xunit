using System;

namespace Xunit
{
    /// <summary>
    /// Represents the configuration items set in the App.config file of a test assembly.
    /// Should be read with the <see cref="ConfigReader"/> class.
    /// </summary>
    public class TestAssemblyConfiguration
    {
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
        /// Gets or sets the maximum number of thread to use when parallelizing this assembly.
        /// </summary>
        public int? MaxParallelThreads { get; set; }

        /// <summary>
        /// Gets the maximum number of thread to use when parallelizing this assembly.
        /// If the value is not set, returns the default value (<see cref="Environment.ProcessorCount"/>).
        /// </summary>
        public int MaxParallelThreadsOrDefault { get { return MaxParallelThreads ?? Environment.ProcessorCount; } }

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
    }
}
