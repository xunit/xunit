using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Runner.Common;

// TODO: These will be replaced by their counterparts in xunit.v3.common/v3/Messages once we replace the message sink.
namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="ITestExecutionSummary"/>.
	/// </summary>
	public class TestExecutionSummary : ITestExecutionSummary, IMessageSinkMessageWithTypes
	{
		static readonly HashSet<string> interfaceTypes = new HashSet<string>(typeof(TestExecutionSummary).GetInterfaces().Select(x => x.FullName!));

		/// <summary>
		/// Initializes a new instance of the <see cref="TestExecutionSummary"/> class.
		/// </summary>
		public TestExecutionSummary(
			TimeSpan elapsedClockTime,
			List<KeyValuePair<string, ExecutionSummary>> summaries)
		{
			ElapsedClockTime = elapsedClockTime;
			Summaries = summaries;
		}

		/// <inheritdoc/>
		public TimeSpan ElapsedClockTime { get; }

		/// <inheritdoc/>
		public HashSet<string> InterfaceTypes => interfaceTypes;

		/// <inheritdoc/>
		public List<KeyValuePair<string, ExecutionSummary>> Summaries { get; }
	}
}
