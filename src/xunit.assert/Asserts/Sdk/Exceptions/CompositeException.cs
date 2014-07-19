using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when an All assertion has one or more items fail an assertion.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class CompositeException : XunitException
    {
        private readonly IReadOnlyList<Tuple<int, Exception>> errors;
        private readonly int totalItems;
 
        /// <summary>
        /// Creates a new instance of the <see cref="CompositeException"/> class.
        /// </summary>
        /// <param name="totalItems">The total number of items that were in the collection.</param>
        /// <param name="errors">The list of errors that occured during the test pass.</param>
        public CompositeException(int totalItems, Tuple<int, Exception>[] errors)
            : base("Assert.All() Failure")
        {
            this.errors = errors;
            this.totalItems = totalItems;
        }

        /// <summary>
        /// The errors that occured during execution of the test.
        /// </summary>
        public IReadOnlyList<Exception> Failures { get { return errors.Select(t => t.Item2).ToList(); } }

        /// <inheritdoc/>
        public override string Message
        {
            get
            {
                var result = new StringBuilder();
                result.AppendLine(String.Format(
                    CultureInfo.CurrentCulture, 
                    "{0} out of {1} items in the collection did not pass.{2}", 
                    errors.Count, 
                    totalItems,
                    Environment.NewLine));

                var first = true;
                foreach (var error in errors)
                {
                    if (!first)
                        result.AppendLine("=================================================================");
                    first = false;

                    result.AppendLine(String.Format(CultureInfo.CurrentCulture,
                                         "{0}{3}Error during validation of item at index {1}{3}Inner exception: {2}",
                                         base.Message,
                                         error.Item1,
                                         error.Item2,
                                         Environment.NewLine));
                }

                return result.ToString();
            }
        }
    }
}