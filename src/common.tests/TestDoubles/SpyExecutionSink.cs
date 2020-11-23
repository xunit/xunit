using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Runner.Common;

namespace Xunit.v3
{
	public class SpyExecutionSink : IExecutionSink
	{
		public ExecutionSummary ExecutionSummary => throw new NotImplementedException();

		public ManualResetEvent Finished => throw new NotImplementedException();

		public List<IMessageSinkMessage> Messages { get; } = new List<IMessageSinkMessage>();

		public void Dispose()
		{ }

		public bool OnMessage(IMessageSinkMessage message)
		{
			Messages.Add(message);
			return true;
		}
	}
}
