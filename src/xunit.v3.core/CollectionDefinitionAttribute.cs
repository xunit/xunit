using System;

namespace Xunit
{
    /// <summary>
    /// Used to declare a test collection container class. The container class gives
    /// developers a place to attach interfaces like <see cref="IClassFixture{T}"/> and
    /// <see cref="ICollectionFixture{T}"/> that will be applied to all tests classes
    /// that are members of the test collection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CollectionDefinitionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionDefinitionAttribute" /> class.
        /// </summary>
        /// <param name="name">The test collection name.</param>
        public CollectionDefinitionAttribute(string name) { }

        /// <summary>
        /// Determines whether tests in this collection runs in parallel with any other collections.
        /// </summary>
        public bool DisableParallelization { get; set; }
    }
}
