using System;
using System.Threading;
using Xunit.Runner.v2;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// Represents an <see cref="IMessageSinkWithTypes"/> that collection execution information and
	/// provides an <see cref="ExecutionSummary" /> once execution is complete.
	/// </summary>
	public interface IExecutionSink : IMessageSinkWithTypes, IDisposable
	{
		/// <summary>
		/// Gets the final execution summary, once the execution is finished.
		/// </summary>
		ExecutionSummary ExecutionSummary { get; }

		/// <summary>
		/// Gets an event which is signaled once execution is finished.
		/// </summary>
		ManualResetEvent Finished { get; }
	}
}
