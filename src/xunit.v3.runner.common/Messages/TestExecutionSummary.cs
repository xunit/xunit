using System;
using System.Collections.Generic;
using System.Linq;

namespace Xunit
{
    /// <summary>
    /// Default implementation of <see cref="ITestExecutionSummary"/>.
    /// </summary>
    public class TestExecutionSummary : ITestExecutionSummary, IMessageSinkMessageWithTypes
    {
        static readonly HashSet<string> interfaceTypes = new HashSet<string>(typeof(TestExecutionSummary).GetInterfaces().Select(x => x.FullName));

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCollectionStarting"/> class.
        /// </summary>
        public TestExecutionSummary(TimeSpan elapsedClockTime, List<KeyValuePair<string, ExecutionSummary>> summaries)
        {
            ElapsedClockTime = elapsedClockTime;
            Summaries = summaries;
        }

        /// <inheritdoc/>
        public TimeSpan ElapsedClockTime { get; private set; }

        /// <inheritdoc/>
        public HashSet<string> InterfaceTypes => interfaceTypes;

        /// <inheritdoc/>
        public List<KeyValuePair<string, ExecutionSummary>> Summaries { get; private set; }
    }
}
