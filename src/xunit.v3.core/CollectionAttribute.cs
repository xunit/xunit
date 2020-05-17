using System;

namespace Xunit
{
    /// <summary>
    /// Used to declare a specific test collection for a test class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class CollectionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionAttribute" /> class.
        /// </summary>
        /// <param name="name">The test collection name.</param>
        public CollectionAttribute(string name) { }
    }
}
