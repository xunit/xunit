#if NET35 || NET452

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// This class provides assistance with assembly resolution for missing assemblies. To help with resolution
    /// of dependencies from a folder, use <see cref="SubscribeResolveForDirectory"/>; for help with resolution
    /// of dependencies from an assembly (with potential use of .deps.json), use <see cref="SubscribeResolveForAssembly"/>.
    /// </summary>
    public class AssemblyHelper : LongLivedMarshalByRefObject, IDisposable
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

            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
        }

        /// <inheritdoc/>
        public void Dispose()
            => AppDomain.CurrentDomain.AssemblyResolve -= Resolve;

        Assembly LoadAssembly(AssemblyName assemblyName)
        {
            if (lookupCache.TryGetValue(assemblyName.Name, out var result))
                return result;

            if (internalDiagnosticsMessageSink != null)
                internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[AssemblyHelper_Desktop.LoadAssembly] Resolving '{assemblyName.Name}'"));

            var path = Path.Combine(directory, assemblyName.Name);
            result = ResolveAndLoadAssembly(path, out var resolvedAssemblyPath);

            if (internalDiagnosticsMessageSink != null)
            {
                if (result == null)
                    internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[AssemblyHelper_Desktop.LoadAssembly] Resolution failed, passed down to next resolver"));
                else
                    internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[AssemblyHelper_Desktop.LoadAssembly] Successful: '{resolvedAssemblyPath}'"));
            }

            lookupCache[assemblyName.Name] = result;
            return result;
        }

        Assembly Resolve(object sender, ResolveEventArgs args)
            => LoadAssembly(new AssemblyName(args.Name));

        Assembly ResolveAndLoadAssembly(string pathWithoutExtension, out string resolvedAssemblyPath)
        {
            foreach (var extension in Extensions)
            {
                resolvedAssemblyPath = pathWithoutExtension + extension;

                try
                {
                    if (File.Exists(resolvedAssemblyPath))
                        return Assembly.LoadFrom(resolvedAssemblyPath);
                }
                catch { }
            }

            resolvedAssemblyPath = null;
            return null;
        }

        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Please use SubscribeResolveForDirectory instead")]
        public static IDisposable SubscribeResolve(string path = null)
            => SubscribeResolveForDirectory(null, null);

        /// <summary>
        /// Subscribes to the appropriate assembly resolution event, to provide automatic assembly resolution for
        /// an assembly and any of its dependencies. Depending on the target platform, this may include the use
        /// of the .deps.json file generated during the build process.
        /// </summary>
        /// <returns>An object which, when disposed, un-subscribes.</returns>
        public static IDisposable SubscribeResolveForAssembly(string assemblyFileName, IMessageSink internalDiagnosticsMessageSink = null)
            => null;    // We don't support .deps.json on Desktop CLR, because it's only available in .NET Core

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
            => new AssemblyHelper(directory ?? Path.GetDirectoryName(typeof(AssemblyHelper).Assembly.GetLocalCodeBase()), internalDiagnosticsMessageSink);
    }
}

#endif
