using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.v3;

namespace Xunit.Sdk
{
	class TestDiscoveryVisitor : _IMessageSink, IDisposable
	{
		bool disposed;

		public TestDiscoveryVisitor()
		{
			Finished = new ManualResetEvent(initialState: false);
			TestCases = new List<_TestCaseDiscovered>();
		}

		public ManualResetEvent Finished { get; }

		public List<_TestCaseDiscovered> TestCases { get; }

		/// <inheritdoc/>
		public void Dispose()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);

			disposed = true;

			Finished.Dispose();
		}

		/// <inheritdoc/>
		public bool OnMessage(_MessageSinkMessage message)
		{
			if (message is _TestCaseDiscovered discoveryMessage)
				TestCases.Add(discoveryMessage);

			if (message is _DiscoveryComplete)
				Finished.Set();

			return true;
		}
	}
}
