#if !ASPNET50

using System;

namespace Xunit
{
    /// <summary/>
    public class AssemblyHelper : IDisposable
    {
        /// <summary/>
        public static IDisposable SubscribeResolve()
        {
            return new AssemblyHelper();
        }

        /// <inheritdoc/>
        public void Dispose() { }
    }
}

#endif
