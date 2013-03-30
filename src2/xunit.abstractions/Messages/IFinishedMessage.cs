
namespace Xunit.Abstractions
{
    /// <summary>
    /// The IFinishedMessage is a base interface for the various types of 
    /// completion that can occur during the execution process. The possible 
    /// scope could be a test case, assembly, etc.  
    /// </summary>
    public interface IFinishedMessage : ITestMessage
    {
        /// <summary>
        /// The execution time is milli-seconds for this execution
        /// </summary>
        decimal ExecutionTime { get; }

        /// <summary>
        /// The number of tests that failed execution
        /// </summary>
        int TestsFailed { get; }

        /// <summary>
        /// The number of tests that were run during execution
        /// </summary>
        int TestsRun { get; }

        /// <summary>
        /// The number of tests skipped during execution
        /// </summary>
        int TestsSkipped { get; }
    }
}
