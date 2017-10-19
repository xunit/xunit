#if NET452

using System;
using System.IO;
using System.Reflection;
using Internal.Microsoft.Extensions.DependencyModel;
using Xunit.Abstractions;

namespace Xunit
{
    class DependencyContextAssemblyHelper : IDisposable
    {
        static readonly DependencyContextJsonReader JsonReader = new DependencyContextJsonReader();

        readonly DependencyContextAssemblyCache assemblyCache;
        readonly IMessageSink internalDiagnosticsMessageSink;

        public DependencyContextAssemblyHelper(string assemblyFolder, DependencyContext dependencyContext, IMessageSink internalDiagnosticsMessageSink)
        {
            this.internalDiagnosticsMessageSink = internalDiagnosticsMessageSink;

            assemblyCache = new DependencyContextAssemblyCache(assemblyFolder, dependencyContext, internalDiagnosticsMessageSink);

            AppDomain.CurrentDomain.AssemblyResolve += OnResolving;
        }

        public void Dispose()
            => AppDomain.CurrentDomain.AssemblyResolve -= OnResolving;

        Assembly OnResolving(object sender, ResolveEventArgs args)
            => assemblyCache.LoadManagedDll(new AssemblyName(args.Name).Name, path => Assembly.LoadFile(path));

        public static IDisposable SubscribeResolveForAssembly(string assemblyFileName, IMessageSink internalDiagnosticsMessageSink)
        {
            var assemblyFolder = Path.GetDirectoryName(assemblyFileName);
            var depsJsonFile = Path.Combine(assemblyFolder, Path.GetFileNameWithoutExtension(assemblyFileName) + ".deps.json");
            if (!File.Exists(depsJsonFile))
            {
                if (internalDiagnosticsMessageSink != null)
                    internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyHelper.SubscribeResolveForAssembly] Skipping resolution for '{depsJsonFile}': File not found"));
                return null;
            }

            using (var stream = File.OpenRead(depsJsonFile))
            {
                var context = JsonReader.Read(stream);
                if (context == null)
                {
                    if (internalDiagnosticsMessageSink != null)
                        internalDiagnosticsMessageSink.OnMessage(new DiagnosticMessage($"[DependencyContextAssemblyHelper.SubscribeResolveForAssembly] Skipping resolution for '{depsJsonFile}': File appears to be malformed"));
                    return null;
                }

                return new DependencyContextAssemblyHelper(assemblyFolder, context, internalDiagnosticsMessageSink);
            }
        }
    }
}

#endif
