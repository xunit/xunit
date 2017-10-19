#if NETCOREAPP1_0

using System;
using System.Runtime.Loader;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// This class provides assistance with assembly resolution for missing assemblies. Runners may
    /// need to use <see cref="SubscribeResolveForAssembly" /> to help automatically resolve missing assemblies
    /// when running tests.
    /// </summary>
    public static class AssemblyHelper
    {
        /// <summary>
        /// Subscribes to the default <see cref="AssemblyLoadContext"/> <see cref="AssemblyLoadContext.Resolving"/> event, to
        /// provide automatic assembly resolution from an assembly which has a .deps.json file from the .NET SDK
        /// build process.
        /// </summary>
        /// <returns>An object which, when disposed, un-subscribes.</returns>
        public static IDisposable SubscribeResolveForAssembly(string assemblyFileName, IMessageSink internalDiagnosticsMessageSink = null)
            => new NetCoreAssemblyDependencyResolver(assemblyFileName, internalDiagnosticsMessageSink);
    }
}

#endif
