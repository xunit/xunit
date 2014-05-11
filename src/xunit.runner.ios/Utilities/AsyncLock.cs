using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Xunit.Runner.iOS.Utilities
{
    internal class AsyncLock
    {
        private readonly SemaphoreSlim semaphore;
        private readonly Task<Releaser> releaser;

        public AsyncLock()
        {
            semaphore = new SemaphoreSlim(1);
            releaser = Task.FromResult(new Releaser(this));
        }

        public struct Releaser : IDisposable
        {
            private readonly AsyncLock toRelease;

            internal Releaser(AsyncLock toRelease) { this.toRelease = toRelease; }

            public void Dispose()
            {
                if (toRelease != null)
                    toRelease.semaphore.Release();
            }
        }

#if DEBUG
        public Task<Releaser> LockAsync([CallerMemberName] string callingMethod = null, [CallerFilePath] string path = null, [CallerLineNumber] int line = 0)
        {
            Debug.WriteLine("AsyncLock.LockAsync called by: " + callingMethod + " in file: " + path + " : " + line);
#else
        public Task<Releaser> LockAsync()
        {
#endif
            var wait = semaphore.WaitAsync();

            return wait.IsCompleted ?
                releaser :
                wait.ContinueWith((_, state) => new Releaser((AsyncLock)state),
                    this, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

    }

}