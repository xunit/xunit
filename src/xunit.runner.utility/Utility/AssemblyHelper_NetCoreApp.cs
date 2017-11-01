#if NETCOREAPP1_0

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// This class provides assistance with assembly resolution for missing assemblies. To help with resolution
    /// of dependencies from a folder, use <see cref="SubscribeResolveForDirectory"/>; for help with resolution
    /// of dependencies from an assembly (with potential use of .deps.json), use <see cref="SubscribeResolveForAssembly"/>.
    /// </summary>
    public class AssemblyHelper : IDisposable
    {
        static readonly string[] Extensions = { ".dll", ".exe" };

        readonly string directory;
        readonly IMessageSink internalDiagnosticsMessageSink;
        readonly Dictionary<string, Assembly> lookupCache = new Dictionary<string, Assembly>();

        /// <summary>
        /// Constructs an instance using the given <paramref name="directory"/> for resolution.
        /// </summary>
        /// <param name="directory">The directory to use for resolving assemblies.</param>
        public AssemblyHelper(string directory) : this(directory, null) { }

        /// <summary>
        /// Constructs an instance using the given <paramref name="directory"/> for resolution.
        /// </summary>
        /// <param name="directory">The directory to use for resolving assemblies.</param>
        /// <param name="internalDiagnosticsMessageSink">The message sink to send internal diagnostics messages to</param>
        public AssemblyHelper(string directory, IMessageSink internalDiagnosticsMessageSink)
        {
            this.directory = directory;
            this.internalDiagnosticsMessageSink = internalDiagnosticsMessageSink;

            AssemblyLoadContext.Default.Resolving += LoadAssembly;
        }

        /// <inheritdoc/>
        public void Dispose()
            => AssemblyLoadContext.Default.Resolving -= LoadAssembly;

        Assembly LoadAssembly(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            if (lookupCache.TryGetValue(assemblyName.Name, out var result))
                return result;

            if (internalDiagnosticsMessageSink != null)
                internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[AssemblyHelper_NetCoreApp.LoadAssembly] Resolving '{assemblyName.Name}'"));

            var path = Path.Combine(directory, assemblyName.Name);
            result = ResolveAndLoadAssembly(context, path, out var resolvedAssemblyPath);

            if (internalDiagnosticsMessageSink != null)
            {
                if (result == null)
                    internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[AssemblyHelper_NetCoreApp.LoadAssembly] Resolution failed, passed down to next resolver"));
                else
                    internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[AssemblyHelper_NetCoreApp.LoadAssembly] Successful: '{resolvedAssemblyPath}'"));
            }

            lookupCache[assemblyName.Name] = result;
            return result;
        }

        Assembly ResolveAndLoadAssembly(AssemblyLoadContext context, string pathWithoutExtension, out string resolvedAssemblyPath)
        {
            foreach (var extension in Extensions)
            {
                resolvedAssemblyPath = pathWithoutExtension + extension;

                try
                {
                    if (File.Exists(resolvedAssemblyPath))
                        return context.LoadFromAssemblyPath(resolvedAssemblyPath);
                }
                catch { }
            }

            resolvedAssemblyPath = null;
            return null;
        }

        /// <summary>
        /// Subscribes to the appropriate assembly resolution event, to provide automatic assembly resolution for
        /// an assembly and any of its dependencies. Depending on the target platform, this may include the use
        /// of the .deps.json file generated during the build process.
        /// </summary>
        /// <returns>An object which, when disposed, un-subscribes.</returns>
        public static IDisposable SubscribeResolveForAssembly(string assemblyFileName, IMessageSink internalDiagnosticsMessageSink = null)
            => new NetCoreAssemblyDependencyResolver(assemblyFileName, internalDiagnosticsMessageSink);

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
            => new AssemblyHelper(directory ?? Path.GetDirectoryName(typeof(AssemblyHelper).GetTypeInfo().Assembly.Location), internalDiagnosticsMessageSink);
    }
}

#endif
