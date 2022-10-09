using System;
using System.Threading;
using Xunit.v3;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents an <see cref="_IMessageSink"/> that collects execution information and
/// provides an <see cref="ExecutionSummary" /> once execution is complete.
/// </summary>
public interface IExecutionSink : _IMessageSink, IDisposable
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
