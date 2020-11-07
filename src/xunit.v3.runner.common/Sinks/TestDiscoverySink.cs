using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// An implementation of <see cref="_IMessageSink"/> designed for test discovery for a
	/// single test assembly. The <see cref="Finished"/> event is triggered when discovery is complete.
	/// </summary>
	public class TestDiscoverySink : _IMessageSink, IDisposable
	{
		readonly Func<bool> cancelThunk;
		readonly DiscoveryEventSink discoverySink = new DiscoveryEventSink();
		bool disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestDiscoverySink"/> class.
		/// </summary>
		/// <param name="cancelThunk">An optional thunk which can be used to control cancellation.</param>
		public TestDiscoverySink(Func<bool>? cancelThunk = null)
		{
			this.cancelThunk = cancelThunk ?? (() => false);

			discoverySink.TestCaseDiscoveryMessageEvent += args =>
			{
				Guard.ArgumentNotNull(nameof(args), args);

				TestCases.Add(args.Message.TestCase);
			};

			discoverySink.DiscoveryCompleteMessageEvent += args => Finished.Set();
		}

		/// <summary>
		/// The list of discovered test cases.
		/// </summary>
		public List<ITestCase> TestCases { get; } = new List<ITestCase>();

		/// <summary>
		/// Gets an event which is signaled once discovery is finished.
		/// </summary>
		public ManualResetEvent Finished { get; } = new ManualResetEvent(initialState: false);

		/// <inheritdoc/>
		public void Dispose()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);

			disposed = true;

			Finished.Dispose();
		}

		/// <inheritdoc/>
		public bool OnMessage(IMessageSinkMessage message)
		{
			Guard.ArgumentNotNull(nameof(message), message);

			return discoverySink.OnMessage(message) && !cancelThunk();
		}
	}
}
