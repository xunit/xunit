using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestCaseOrderer"/>. Orders tests in
    /// an unpredictable but stable order, so that repeated test runs of the
    /// identical test assembly run tests in the same order.
    /// </summary>
    public class DefaultTestCaseOrderer : ITestCaseOrderer
    {
        IMessageSink diagnosticMessageSink;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTestCaseOrderer"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">Message sink to report diagnostic messages to</param>
        public DefaultTestCaseOrderer(IMessageSink diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        /// <inheritdoc/>
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
        {
            var result = testCases.ToList();

            try
            {
                result.Sort(Compare);
            }
            catch (Exception ex)
            {
                diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Exception thrown in DefaultTestCaseOrderer.OrderTestCases(); falling back to random order.{Environment.NewLine}{ex}"));
                result = Randomize(result);
            }

            return result;
        }

        List<TTestCase> Randomize<TTestCase>(List<TTestCase> testCases)
        {
            var result = new List<TTestCase>(testCases.Count);
            var randomizer = new Random();

            while (testCases.Count > 0)
            {
                var next = randomizer.Next(testCases.Count);
                result.Add(testCases[next]);
                testCases.RemoveAt(next);
            }

            return result;
        }

        int Compare<TTestCase>(TTestCase x, TTestCase y)
            where TTestCase : ITestCase
        {
            Guard.ArgumentNotNull(nameof(x), x);
            Guard.ArgumentNotNull(nameof(y), y);
            Guard.ArgumentValid(nameof(x), $"Could not compare test case {x.DisplayName} because it has a null UniqueID", x.UniqueID != null);
            Guard.ArgumentValid(nameof(y), $"Could not compare test case {y.DisplayName} because it has a null UniqueID", y.UniqueID != null);

            return string.CompareOrdinal(x.UniqueID, y.UniqueID);
        }
    }
}
