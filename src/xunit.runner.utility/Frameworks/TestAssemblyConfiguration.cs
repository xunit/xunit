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
        /// Initializes a new instance of the <see cref="TestAssemblyConfiguration"/> class.
        /// </summary>
        public TestAssemblyConfiguration()
        {
            MaxParallelThreads = Environment.ProcessorCount;
            MethodDisplay = TestMethodDisplay.NamespaceAndClassAndMethod;
            ParallelizeTestCollections = true;
        }

        /// <summary>
        /// Gets or sets a flag indicating that the end user wants diagnostic messages
        /// from the test framework. Defaults to <c>false</c>.
        /// </summary>
        public bool DiagnosticMessages { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of thread to use when parallelizing this assembly.
        /// Defaults to <see cref="Environment.ProcessorCount"/>.
        /// </summary>
        public int MaxParallelThreads { get; set; }

        /// <summary>
        /// Gets or sets the default display name for test methods. Defaults
        /// to <see cref="TestMethodDisplay.NamespaceAndClassAndMethod"/>.
        /// </summary>
        public TestMethodDisplay MethodDisplay { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating that this assembly is safe to parallelize against
        /// other assemblies. Defaults to <c>false</c>.
        /// </summary>
        public bool ParallelizeAssembly { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating that this test assembly wants to run test collections
        /// in parallel against one another. Defaults to <c>true</c>.
        /// </summary>
        public bool ParallelizeTestCollections { get; set; }
    }
}