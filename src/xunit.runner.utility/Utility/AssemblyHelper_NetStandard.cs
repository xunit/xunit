#if NETSTANDARD1_1 || NETSTANDARD1_5

using System;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// This class provides assistance with assembly resolution for missing assemblies. To help with resolution
    /// of dependencies from a folder, use <see cref="SubscribeResolveForDirectory"/>; for help with resolution
    /// of dependencies from an assembly (with potential use of .deps.json), use <see cref="SubscribeResolveForAssembly"/>.
    /// </summary>
    public class AssemblyHelper
    {
        /// <summary>
        /// Subscribes to the appropriate assembly resolution event, to provide automatic assembly resolution for
        /// an assembly and any of its dependencies. Depending on the target platform, this may include the use
        /// of the .deps.json file generated during the build process.
        /// </summary>
        /// <returns>An object which, when disposed, un-subscribes.</returns>
        public static IDisposable SubscribeResolveForAssembly(string assemblyFileName, IMessageSink internalDiagnosticsMessageSink = null)
            => null;    // We don't support .deps.json on .NET Standard, because it's only available in .NET Core

        /// <summary>
        /// Subscribes to the appropriate assembly resolution event, to provide automatic assembly resolution for
        /// any assemblies located in a folder. This is typically used to help resolve the dependencies of
        /// the runner utility library from its location in the NuGet packages folder.
        /// </summary>
        /// <param name="internalDiagnosticsMessageSink">The optional message sink to send internal diagnostics messages to.</param>
        /// <returns>An object which, when disposed, un-subscribes.</returns>
        /// <param name="directory">The optional directory to use for resolving assemblies; if <c>null</c> is passed,
        /// then it uses the directory which contains the runner utility assembly.</param>
        public static IDisposable SubscribeResolveForDirectory(IMessageSink internalDiagnosticsMessageSink = null, string directory = null)
            => null;    // We don't support this in .NET Standard, because we assume the packaging system does this for us
    }
}

#endif
