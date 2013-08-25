using System;
using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Used to declare a the default test collection behavior for the assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class CollectionBehaviorAttribute : AttributeBase
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

        // This method is here as an allowance to Enum-issues related to CustomAttributeData.
        [EditorBrowsable(EditorBrowsableState.Never)]
        public CollectionBehaviorAttribute(int collectionBehavior) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionBehaviorAttribute" /> class.
        /// </summary>
        /// <param name="factoryTypeName">The type name of the test collection factory (that implements <see cref="IXunitTestCollectionFactory"/>.</param>
        /// <param name="factoryAssemblyName">The assembly that <paramref name="factoryTypeName"/> exists in.</param>
        public CollectionBehaviorAttribute(string factoryTypeName, string factoryAssemblyName) { }

        /// <summary>
        /// Determines whether tests in this assembly are run in parallel.
        /// </summary>
        public bool RunCollectionsInParallel { get; set; }
    }
}