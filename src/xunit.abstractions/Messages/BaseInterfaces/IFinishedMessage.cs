namespace Xunit.Abstractions
{
    /// <summary>
    /// This is the base message for various types of completion that can occur during the
    /// various phases of execution process (e.g.,  test case, test class, test collection,
    /// and assembly).
    /// </summary>
    public interface IFinishedMessage : IMessageSinkMessage
    {
        /// <summary>
        /// The execution time (in seconds) for this execution.
        /// </summary>
        decimal ExecutionTime { get; }

        /// <summary>
        /// The number of failing tests.
        /// </summary>
        int TestsFailed { get; }

        /// <summary>
        /// The total number of tests run.
        /// </summary>
        int TestsRun { get; }

        /// <summary>
        /// The number of skipped tests.
        /// </summary>
        int TestsSkipped { get; }
    }
}