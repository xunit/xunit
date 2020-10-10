using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xunit.Sdk
{
	/// <summary>
	/// Tracks disposable objects, and disposes them in the reverse order they were added to
	/// the tracker. Supports both <see cref="IDisposable"/> and <see cref="IAsyncDisposable"/>.
	/// </summary>
	public class DisposalTracker : IAsyncDisposable
	{
		bool disposed;
		readonly Stack<IDisposable> toDispose = new Stack<IDisposable>();
		readonly Stack<IAsyncDisposable> toAsyncDispose = new Stack<IAsyncDisposable>();

		/// <summary>
		/// Add an object to be disposed. It may optionally support <see cref="IDisposable"/>
		/// and/or <see cref="IAsyncDisposable"/>.
		/// </summary>
		/// <param name="obj">The object to be disposed.</param>
		public void Add(object? obj)
		{
			lock (toDispose)
			{
				GuardNotDisposed();

				if (obj is IDisposable disposable)
					toDispose.Push(disposable);
				if (obj is IAsyncDisposable asyncDisposable)
					toAsyncDispose.Push(asyncDisposable);
			}
		}

		/// <inheritdoc/>
		public async ValueTask DisposeAsync()
		{
			lock (toDispose)
			{
				GuardNotDisposed();
				disposed = true;
			}

			foreach (var asyncDisposable in toAsyncDispose)
				await asyncDisposable.DisposeAsync();

			foreach (var disposable in toDispose)
				disposable.Dispose();
		}

		void GuardNotDisposed()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);
		}
	}
}
