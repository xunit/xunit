using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Runner.Common;
using Xunit.v3;

public class SpyExecutionSink : IExecutionSink
{
	public ExecutionSummary ExecutionSummary => throw new NotImplementedException();

	public ManualResetEvent Finished => throw new NotImplementedException();

	public List<_MessageSinkMessage> Messages { get; } = new List<_MessageSinkMessage>();

	public void Dispose()
	{ }

	public bool OnMessage(_MessageSinkMessage message)
	{
		Messages.Add(message);
		return true;
	}
}
