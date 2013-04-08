using System;
using System.Collections.Generic;

namespace Xunit.Sdk
{
    /// <summary>
    /// Aggregates exceptions. Intended to run one or more code blocks, and collect the
    /// exceptions thrown by those code blocks.
    /// </summary>
    public class ExceptionAggregator
    {
        List<Exception> exceptions = new List<Exception>();

        /// <summary>
        /// Runs the code, catching the exception that is thrown and adding it to
        /// the aggregate.
        /// </summary>
        /// <param name="code">The code to be run.</param>
        public void Run(Action code)
        {
            try
            {
                code();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex.Unwrap());
            }
        }

        /// <summary>
        /// Returns an exception that represents the exceptions thrown by the code
        /// passed to the <see cref="Run"/> method.
        /// </summary>
        /// <returns>Returns <c>null</c> if no exceptions were thrown; returns the
        /// exact exception is a single exception was thrown; returns <see cref="AggregateException"/>
        /// if more than one exception was thrown.</returns>
        public Exception ToException()
        {
            if (exceptions.Count == 0)
                return null;
            if (exceptions.Count == 1)
                return exceptions[0];
            return new AggregateException(exceptions);
        }
    }
}