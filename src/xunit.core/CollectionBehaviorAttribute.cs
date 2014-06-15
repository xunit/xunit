using System;

namespace Xunit
{
    /// <summary>
    /// Used to declare a the default test collection behavior for the assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class CollectionBehaviorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionBehaviorAttribute" /> class.
        /// </summary>
        public CollectionBehaviorAttribute() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionBehaviorAttribute" /> class.
        /// </summary>
        /// <param name="collectionBehavior">The collection behavior for the assembly.</param>
        public CollectionBehaviorAttribute(CollectionBehavior collectionBehavior) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionBehaviorAttribute" /> class.
        /// </summary>
        /// <param name="factoryTypeName">The type name of the test collection factory (that implements <see cref="T:Xunit.Sdk.IXunitTestCollectionFactory"/>).</param>
        /// <param name="factoryAssemblyName">The assembly that <paramref name="factoryTypeName"/> exists in.</param>
        public CollectionBehaviorAttribute(string factoryTypeName, string factoryAssemblyName) { }

        /// <summary>
        /// Determines whether tests in this assembly are run in parallel.
        /// </summary>
        public bool DisableTestParallelization { get; set; }

        /// <summary>
        /// Determines how many tests can run in parallel with each other.
        /// </summary>
        public int MaxParallelThreads { get; set; }
    }
}