using System;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="ITestCollection"/> that is used for xUnit.net v1 tests. A single test
    /// collection is used for the entire test assembly, since xUnit.net v1 does not support parallelization.
    /// </summary>
    public class Xunit1TestCollection : ITestCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Xunit1TestCollection" /> class.
        /// </summary>
        /// <param name="assemblyFileName">The assembly under test.</param>
        public Xunit1TestCollection(string assemblyFileName)
        {
            DisplayName = String.Format("xUnit.net v1 Tests for {0}", assemblyFileName);
            ID = Guid.NewGuid();
        }

        /// <inheritdoc/>
        public ITypeInfo CollectionDefinition { get; private set; }

        /// <inheritdoc/>
        public string DisplayName { get; private set; }

        /// <inheritdoc/>
        public Guid ID { get; private set; }
    }
}