using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Runner.Common;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// An implementation of <see cref="IMessageSink"/> and <see cref="IMessageSinkWithTypes"/> which
	/// collects native xUnit.net v2 test cases, for use with <see cref="Xunit2.FindAndRun"/>.
	/// </summary>
	public class Xunit2DiscoverySink : LongLivedMarshalByRefObject, IMessageSink, IMessageSinkWithTypes
	{
		readonly Xunit2MessageAdapter adapter;
		bool disposed;
		readonly XunitFilters filters;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit2DiscoverySink"/> class.
		/// </summary>
		/// <param name="filters">The filters to be applied to the discovered test cases</param>
		public Xunit2DiscoverySink(XunitFilters filters)
		{
			this.filters = filters;

			adapter = new Xunit2MessageAdapter();
		}

		/// <summary>
		/// Gets an event which is signaled once discovery is finished.
		/// </summary>
		public ManualResetEvent Finished { get; } = new ManualResetEvent(initialState: false);

		/// <summary>
		/// The list of discovered test cases.
		/// </summary>
		public List<ITestCase> TestCases { get; } = new();

		void Dispatch<TMessage>(
			IMessageSinkMessage message,
			HashSet<string>? messageTypes,
			Action<TMessage> handler)
				where TMessage : class, IMessageSinkMessage
		{
			var castMessage = messageTypes == null || messageTypes.Contains(typeof(TMessage).FullName!) ? message as TMessage : null;
			if (castMessage != null)
				handler(castMessage);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);

			disposed = true;

			Finished.Dispose();
		}

		void HandleDiscoveryComplete(IDiscoveryCompleteMessage message)
		{
			if (disposed)
				return;

			Finished.Set();
		}

		void HandleTestCaseDiscovery(ITestCaseDiscoveryMessage message)
		{
			if (disposed)
				return;

			if (filters.Empty || (adapter.Adapt(message) is _TestCaseDiscovered adapted && filters.Filter(adapted)))
				TestCases.Add(message.TestCase);
		}

		/// <inheritdoc/>
		public bool OnMessage(IMessageSinkMessage message) =>
			OnMessageWithTypes(message, null);

		/// <inheritdoc/>
		public bool OnMessageWithTypes(
			IMessageSinkMessage message,
			HashSet<string>? messageTypes)
		{
			Dispatch<IDiscoveryCompleteMessage>(message, messageTypes, HandleDiscoveryComplete);
			Dispatch<ITestCaseDiscoveryMessage>(message, messageTypes, HandleTestCaseDiscovery);

			return true;
		}
	}
}
