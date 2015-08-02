using System;

#if !DOTNETCORE
using System.Threading;
#endif

namespace Xunit.Sdk
{
    /// <summary>
    /// A wrapper around ExecutionContext, which is not directly available in DotNetCore in a way
    /// that is compatible with dnx451. Any .NET Core runners must set the <see cref="Capture"/>
    /// and <see cref="Run"/> properties before <see cref="MaxConcurrencySyncContext"/> will
    /// function correctly. This requirement will be removed once support for dnx451 is removed
    /// in favor of dnx46.
    /// </summary>
    public static class ExecutionContextWrapper
    {
#if !DOTNETCORE
        static ExecutionContextWrapper()
        {
            Capture = ExecutionContext.Capture;
            Run = (context, code) => ExecutionContext.Run((ExecutionContext)context, _ => code(), null);
        }
#endif

        /// <summary>
        /// A wrapper around <see cref="M:System.Threading.ExecutionContext.Capture"/>.
        /// </summary>
        public static Func<object> Capture { get; set; }

        /// <summary>
        /// A wrapper around <see cref="M:System.Threading.ExecutionContext.Run"/>.
        /// </summary>
        public static Action<object, Action> Run { get; set; }
    }
}
