using System;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// A default implementation of <see cref="_ITestFramework"/> that tracks objects to be
	/// disposed when the framework is disposed. The discoverer and executor are automatically
	/// tracked for disposal, since those interfaces mandate an implementation of <see cref="IDisposable"/>.
	/// </summary>
	public abstract class TestFramework : _ITestFramework, IAsyncDisposable
	{
		bool disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestFramework"/> class.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		protected TestFramework(_IMessageSink diagnosticMessageSink)
		{
			DiagnosticMessageSink = Guard.ArgumentNotNull(nameof(diagnosticMessageSink), diagnosticMessageSink);
		}

		/// <summary>
		/// Gets the message sink used to send diagnostic messages.
		/// </summary>
		public _IMessageSink DiagnosticMessageSink { get; }

		/// <summary>
		/// Gets the disposal tracker for the test framework.
		/// </summary>
		protected DisposalTracker DisposalTracker { get; } = new();

		/// <inheritdoc/>
		public virtual async ValueTask DisposeAsync()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);

			disposed = true;

			await ExtensibilityPointFactory.DisposeAsync();
			await DisposalTracker.DisposeAsync();
		}

		/// <summary>
		/// Override this method to provide the implementation of <see cref="_ITestFrameworkDiscoverer"/>.
		/// </summary>
		/// <param name="assembly">The assembly that is being discovered.</param>
		/// <returns>Returns the test framework discoverer.</returns>
		protected abstract _ITestFrameworkDiscoverer CreateDiscoverer(_IAssemblyInfo assembly);

		/// <summary>
		/// Override this method to provide the implementation of <see cref="_ITestFrameworkExecutor"/>.
		/// </summary>
		/// <param name="assembly">The assembly that is being executed.</param>
		/// <returns>Returns the test framework executor.</returns>
		protected abstract _ITestFrameworkExecutor CreateExecutor(_IReflectionAssemblyInfo assembly);

		/// <inheritdoc/>
		public _ITestFrameworkDiscoverer GetDiscoverer(_IAssemblyInfo assembly)
		{
			Guard.ArgumentNotNull(nameof(assembly), assembly);

			var discoverer = CreateDiscoverer(assembly);
			DisposalTracker.Add(discoverer);
			return discoverer;
		}

		/// <inheritdoc/>
		public _ITestFrameworkExecutor GetExecutor(_IReflectionAssemblyInfo assembly)
		{
			Guard.ArgumentNotNull(nameof(assembly), assembly);

			var executor = CreateExecutor(assembly);
			DisposalTracker.Add(executor);
			return executor;
		}
	}
}
