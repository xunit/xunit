using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Xunit.Sdk
{
    /// <summary>
    /// Measures and aggregates execution time of one or more actions.
    /// </summary>
    public class ExecutionTimer
    {
        TimeSpan total;

        /// <summary>
        /// Returns the total time aggregated across all the actions.
        /// </summary>
        public decimal Total
        {
            get { return (decimal)total.TotalSeconds; }
        }

        /// <summary>
        /// Executes an action and aggregates its run time into the total.
        /// </summary>
        /// <param name="action">The action to measure.</param>
        public void Aggregate(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                action();
            }
            finally
            {
                total += stopwatch.Elapsed;
            }
        }

        /// <summary>
        /// Executes an asynchronous action and aggregates its run time into the total.
        /// </summary>
        /// <param name="asyncAction">The action to measure.</param>
        public async Task AggregateAsync(Func<Task> asyncAction)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await asyncAction();
            }
            finally
            {
                total += stopwatch.Elapsed;
            }
        }

        /// <summary>
        /// Aggregates a time span into the total time.
        /// </summary>
        /// <param name="time">The time to add.</param>
        public void Aggregate(TimeSpan time)
        {
            total += time;
        }
    }
}
