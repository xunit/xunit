using System;
using System.Collections.Generic;

namespace Xunit.Sdk
{
    /// <summary>
    /// Tracks disposable objects, and disposes them in the reverse order they were added to
    /// the tracker.
    /// </summary>
    public class DisposalTracker : IDisposable
    {
        readonly Stack<IDisposable> toDispose = new Stack<IDisposable>();

        /// <summary>
        /// Add an object to be disposed.
        /// </summary>
        /// <param name="disposable">The object to be disposed.</param>
        public void Add(IDisposable disposable)
        {
            toDispose.Push(disposable);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var disposable in toDispose)
                disposable.Dispose();

            toDispose.Clear();
        }
    }
}
