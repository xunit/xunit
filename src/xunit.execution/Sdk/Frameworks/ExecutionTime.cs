using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Xunit.Sdk
{
    /// <summary>
    /// Measures and aggregates execution time of one or more actions.
    /// </summary>
    public class ExecutionTime
    {
        /// <summary>
        /// Returns the total time aggregated across all the actions.
        /// </summary>
        public TimeSpan Total { get; private set; }

        /// <summary>
        /// Executes an action and aggregates its run time into the total.
        /// </summary>
        /// <param name="action">The action to measure.</param>
        public void Aggregate(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            Total += stopwatch.Elapsed;
        }

        /// <summary>
        /// Executes an asynchronous action and aggregates its run time into the total.
        /// </summary>
        /// <param name="asyncAction">The action to measure.</param>
        public async Task AggregateAsync(Func<Task> asyncAction)
        {
            var stopwatch = Stopwatch.StartNew();
            await asyncAction();
            Total += stopwatch.Elapsed;
        }
    }
}
