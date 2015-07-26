namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when the collection did not contain exactly one element.
    /// </summary>
    public class SingleException : AssertCollectionCountException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleException"/> class.
        /// </summary>
        /// <param name="count">The numbers of items in the collection.</param>
        public SingleException(int count) : base(1, count) { }
    }
}
