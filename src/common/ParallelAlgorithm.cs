#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Indicates the parallelization algorithm to use.
    /// </summary>
    public enum ParallelAlgorithm
    {
        /// <summary>
        /// The conservative parallelization algorithm uses a semaphore to limit the number of started tests to be equal
        /// to the desired parallel thread count. This has the effect of allowing tests that have started to finish faster,
        /// since there are no extra tests competing for a chance to run, at the expense that CPU utilization will be lowered
        /// if the test project spaws a lot of async tests that have significant wait times.
        /// </summary>
        Conservative = 0,

        /// <summary>
        /// The aggressive parallelization algorithm uses a synchronization context to limit the number of running tests
        /// to be equal to the desired parallel thread count. This has the effect of being able to use the CPU more
        /// effectively since there are typically most tests capable of running than there are CPU cores, at the
        /// expense of tests that have already started being put into the back of a long queue before they can run
        /// again.
        /// </summary>
        Aggressive = 1,
    }
}
