namespace Xunit
{
    /// <summary>
    /// Defines the built-in behavior types for collections in xUnit.net.
    /// </summary>
    public enum CollectionBehavior
    {
        /// <summary>
        /// By default, generates a collection per assembly, and any test classes that are not
        /// decorated with <see cref="CollectionAttribute"/> will be placed into the assembly-level
        /// collection.
        /// </summary>
        CollectionPerAssembly,

        /// <summary>
        /// By default, generates a collection per test class for any test classes that are not
        /// decorated with <see cref="CollectionAttribute"/>.
        /// </summary>
        CollectionPerClass
    }
}
