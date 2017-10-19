#if NET35 || NET452

using System;
using System.IO;
using System.Reflection;

namespace Xunit
{
    /// <summary>
    /// This class provides assistance with assembly resolution for missing assemblies. Runners may
    /// need to use <see cref="SubscribeResolve(string)" /> to help automatically resolve missing assemblies
    /// when running tests.
    /// </summary>
    public class AssemblyHelper : LongLivedMarshalByRefObject, IDisposable
    {
        readonly string directory;

        /// <summary>
        /// Constructs an instance using the given <paramref name="directory"/> for resolution.
        /// </summary>
        /// <param name="directory">The directory to use for resolving assemblies.</param>
        public AssemblyHelper(string directory)
        {
            this.directory = directory;

            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
        }

        /// <inheritdoc/>
        public void Dispose()
            => AppDomain.CurrentDomain.AssemblyResolve -= Resolve;

        Assembly LoadAssembly(AssemblyName assemblyName)
        {
            var path = Path.Combine(directory, assemblyName.Name);
            return LoadAssembly(path + ".dll") ?? LoadAssembly(path + ".exe");
        }

        static Assembly LoadAssembly(string assemblyPath)
        {
            try
            {
                if (File.Exists(assemblyPath))
                    return Assembly.LoadFrom(assemblyPath);
            }
            catch { }

            return null;
        }

        Assembly Resolve(object sender, ResolveEventArgs args)
            => LoadAssembly(new AssemblyName(args.Name));

        /// <summary>
        /// Subscribes to the current <see cref="AppDomain"/> <see cref="AppDomain.AssemblyResolve"/> event, to
        /// provide automatic assembly resolution for assemblies in the runner.
        /// </summary>
        /// <returns>An object which, when disposed, un-subscribes.</returns>
        public static IDisposable SubscribeResolve(string path = null)
            => new AssemblyHelper(path ?? Path.GetDirectoryName(typeof(AssemblyHelper).Assembly.GetLocalCodeBase()));

#if NET452
        /// <summary>
        /// Subscribes to the current <see cref="AppDomain"/> <see cref="AppDomain.AssemblyResolve"/> event, to
        /// provide automatic assembly resolution from an assembly which has a .deps.json file from the .NET SDK
        /// build process.
        /// </summary>
        /// <returns>An object which, when disposed, un-subscribes.</returns>
        public static IDisposable SubscribeResolveForAssembly(string assemblyFileName, Xunit.Abstractions.IMessageSink internalDiagnosticsMessageSink = null)
            => DependencyContextAssemblyHelper.SubscribeResolveForAssembly(assemblyFileName, internalDiagnosticsMessageSink);
#endif
    }
}

#endif
