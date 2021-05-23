using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Xunit.Internal;

namespace Xunit.Sdk
{
	/// <summary>
	/// Tracks disposable objects, and disposes them in the reverse order they were added to
	/// the tracker. Supports both <see cref="IDisposable"/> and <see cref="IAsyncDisposable"/>.
	/// You can either directly dispose this object (via <see cref="DisposeAsync"/>), or you
	/// can enumerate the items contained inside of it (via <see cref="TrackedObjects"/>).
	/// Also supports hand-registering disposal actions via <see cref="AddAction"/>
	/// and <see cref="AddAsyncAction"/>. Note that an object implements both interfaces,
	/// this will *only* call <see cref="IAsyncDisposable.DisposeAsync"/> and will not
	/// call <see cref="IDisposable.Dispose"/>.
	/// </summary>
	public class DisposalTracker : IAsyncDisposable
	{
		bool disposed;
		readonly Stack<object> trackedObjects = new();

		/// <summary>
		/// Gets a list of the items that are currently being tracked.
		/// </summary>
		public IReadOnlyCollection<object> TrackedObjects
		{
			get
			{
				lock (trackedObjects)
				{
					GuardNotDisposed();

					return trackedObjects.ToList();
				}
			}
		}

		/// <summary>
		/// Add an object to be disposed. It may optionally support <see cref="IDisposable"/>
		/// and/or <see cref="IAsyncDisposable"/>.
		/// </summary>
		/// <param name="object">The object to be disposed.</param>
		public void Add(object? @object)
		{
			lock (trackedObjects)
			{
				GuardNotDisposed();

				AddInternal(@object);
			}
		}

		/// <summary>
		/// Add an action to the list of things to be done during disposal.
		/// </summary>
		/// <param name="cleanupAction">The cleanup action.</param>
		public void AddAction(Action cleanupAction) =>
			Add(new DisposableWrapper(cleanupAction));

		/// <summary>
		/// Add an action to the list of things to be done during disposal.
		/// </summary>
		/// <param name="cleanupAction">The cleanup action.</param>
		public void AddAsyncAction(Func<ValueTask> cleanupAction) =>
			Add(new AsyncDisposableWrapper(cleanupAction));

		void AddInternal(object? @object)
		{
			if (@object != null)
				trackedObjects.Push(@object);
		}

		/// <summary>
		/// Add a collection of objects to be disposed. They may optionally support <see cref="IDisposable"/>
		/// and/or <see cref="IAsyncDisposable"/>.
		/// </summary>
		/// <param name="objects">The objects to be disposed.</param>
		public void AddRange(object?[]? objects)
		{
			lock (trackedObjects)
			{
				GuardNotDisposed();

				if (objects != null)
					foreach (var @object in objects)
						AddInternal(@object);
			}
		}

		/// <summary>
		/// Removes all objects from the disposal tracker.
		/// </summary>
		public void Clear()
		{
			lock (trackedObjects)
			{
				GuardNotDisposed();

				trackedObjects.Clear();
			}
		}

		/// <summary>
		/// Disposes all the objects that were added to the disposal tracker, in the reverse order
		/// of which they were added. For any object which implements both <see cref="IDisposable"/>
		/// and <see cref="IAsyncDisposable"/> will have <see cref="IAsyncDisposable.DisposeAsync"/>
		/// called first, and then <see cref="IDisposable.Dispose"/> called after.
		/// </summary>
		public async ValueTask DisposeAsync()
		{
			lock (trackedObjects)
			{
				GuardNotDisposed();

				disposed = true;
			}

			var exceptions = new List<Exception>();

			foreach (var trackedObject in trackedObjects)
			{
				try
				{
					if (trackedObject is IAsyncDisposable asyncDisposable)
						await asyncDisposable.DisposeAsync();
					else if (trackedObject is IDisposable disposable)
						disposable.Dispose();
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			}

			if (exceptions.Count == 1)
				ExceptionDispatchInfo.Capture(exceptions[0]).Throw();
			if (exceptions.Count != 0)
				throw new AggregateException(exceptions);
		}

		void GuardNotDisposed()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);
		}

		class AsyncDisposableWrapper : IAsyncDisposable
		{
			readonly Func<ValueTask> cleanupAction;

			public AsyncDisposableWrapper(Func<ValueTask> cleanupAction) =>
				this.cleanupAction = Guard.ArgumentNotNull(nameof(cleanupAction), cleanupAction);

			public ValueTask DisposeAsync() =>
				cleanupAction();
		}

		class DisposableWrapper : IDisposable
		{
			readonly Action cleanupAction;

			public DisposableWrapper(Action cleanupAction) =>
				this.cleanupAction = Guard.ArgumentNotNull(nameof(cleanupAction), cleanupAction);

			public void Dispose() =>
				cleanupAction();
		}
	}
}
