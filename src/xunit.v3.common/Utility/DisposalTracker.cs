using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xunit.Sdk
{
	/// <summary>
	/// Tracks disposable objects, and disposes them in the reverse order they were added to
	/// the tracker. Supports both <see cref="IDisposable"/> and <see cref="IAsyncDisposable"/>.
	/// You can either directly dispose this object (via <see cref="DisposeAsync"/>), or you
	/// can enumerate the items contained inside of it (via <see cref="Disposables"/> and
	/// <see cref="AsyncDisposables"/>).
	/// </summary>
	public class DisposalTracker : IAsyncDisposable
	{
		bool disposed;
		readonly Stack<IDisposable> toDispose = new Stack<IDisposable>();
		readonly Stack<IAsyncDisposable> toAsyncDispose = new Stack<IAsyncDisposable>();

		/// <summary>
		/// Gets a list of the async disposable items (and then clears the list).
		/// </summary>
		public IEnumerable<IAsyncDisposable> AsyncDisposables
		{
			get
			{
				List<IAsyncDisposable> result;

				lock (toDispose)
				{
					GuardNotDisposed();

					result = toAsyncDispose.ToList();
					toAsyncDispose.Clear();
				}

				return result;
			}
		}

		/// <summary>
		/// Gets a list of the disposable items (and then clears the list).
		/// </summary>
		public IEnumerable<IDisposable> Disposables
		{
			get
			{
				List<IDisposable> result;

				lock (toDispose)
				{
					GuardNotDisposed();

					result = toDispose.ToList();
					toDispose.Clear();
				}

				return result;
			}
		}

		/// <summary>
		/// Add an object to be disposed. It may optionally support <see cref="IDisposable"/>
		/// and/or <see cref="IAsyncDisposable"/>.
		/// </summary>
		/// <param name="object">The object to be disposed.</param>
		public void Add(object? @object)
		{
			lock (toDispose)
			{
				GuardNotDisposed();

				if (@object is IDisposable disposable)
					toDispose.Push(disposable);
				if (@object is IAsyncDisposable asyncDisposable)
					toAsyncDispose.Push(asyncDisposable);
			}
		}

		/// <summary>
		/// Add a collection of objects to be disposed. They may optionally support <see cref="IDisposable"/>
		/// and/or <see cref="IAsyncDisposable"/>.
		/// </summary>
		/// <param name="objects">The objects to be disposed.</param>
		public void AddRange(object?[]? objects)
		{
			if (objects != null)
				foreach (var @object in objects)
					Add(@object);
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
