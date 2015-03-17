using System;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents a group of test cases. Test collections form the basis of the parallelization in
    /// xUnit.net. Test cases which are in the same test collection will not be run in parallel
    /// against sibling tests, but will run in parallel against tests in other collections.
    /// </summary>
    public interface ITestCollection : IXunitSerializable
    {
        /// <summary>
        /// Gets the type that the test collection was defined with, if available; may be <c>null</c>
        /// if the test collection didn't have a definition type.
        /// </summary>
        ITypeInfo CollectionDefinition { get; }

        /// <summary>
        /// Gets the display name of the test collection.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the test assembly this test collection belongs to.
        /// </summary>
        ITestAssembly TestAssembly { get; }

        /// <summary>
        /// Gets the test collection ID. Test collection equality is determined by comparing IDs.
        /// </summary>
        Guid UniqueID { get; }
    }
}