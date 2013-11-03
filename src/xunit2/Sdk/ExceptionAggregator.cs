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
        readonly List<Exception> exceptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionAggregator"/> class.
        /// </summary>
        public ExceptionAggregator()
        {
            exceptions = new List<Exception>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionAggregator"/> class that
        /// contains the exception list of its parent.
        /// </summary>
        /// <param name="parent">The parent aggregator to copy exceptions from.</param>
        public ExceptionAggregator(ExceptionAggregator parent)
        {
            exceptions = new List<Exception>(parent.exceptions);
        }

        /// <summary>
        /// Returns <c>true</c> if the aggregator has at least one exception inside it.
        /// </summary>
        public bool HasExceptions { get { return exceptions.Count > 0; } }

        /// <summary>
        /// Adds an exception to the aggregator.
        /// </summary>
        /// <param name="ex">The exception to be added.</param>
        public void Add(Exception ex)
        {
            exceptions.Add(ex);
        }

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