namespace Xunit.Abstractions
{
    /// <summary>
    /// This represents failure information for the test runner. It encapsulates multiple sets
    /// of exceptions so that it can provide inner exception information, including support for
    /// <see cref="T:System.AggregateException"/>. The parent indices indicate the hierarchy of the exceptions
    /// as extracted during the failure; the 0th exception is always the single parent of the tree,
    /// and will have an index of -1.
    /// </summary>
    public interface IFailureInformation
    {
        /// <summary>
        /// The fully-qualified type name of the exceptions.
        /// </summary>
        string[] ExceptionTypes { get; }

        /// <summary>
        /// The messages of the exceptions.
        /// </summary>
        string[] Messages { get; }

        /// <summary>
        /// The stack traces of the exceptions.
        /// </summary>
        string[] StackTraces { get; }

        /// <summary>
        /// The parent exception index for the exceptions; a -1 indicates that
        /// the exception in question has no parent.
        /// </summary>
        int[] ExceptionParentIndices { get; }
    }
}
