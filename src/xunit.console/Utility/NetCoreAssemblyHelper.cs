#if NETCOREAPP1_0

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Xunit
{
    public class NetCoreAssemblyHelper : IDisposable
    {
        readonly string directory;

        /// <summary>
        /// Constructs an instance using the given <paramref name="directory"/> for resolution.
        /// </summary>
        /// <param name="directory">The directory to use for resolving assemblies.</param>
        public NetCoreAssemblyHelper(string directory)
        {
            this.directory = directory;

            AssemblyLoadContext.Default.Resolving += LoadAssembly;
        }

        public void Dispose()
            => AssemblyLoadContext.Default.Resolving -= LoadAssembly;

        Assembly LoadAssembly(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            var path = Path.Combine(directory, assemblyName.Name);
            return LoadAssembly(context, path + ".dll") ?? LoadAssembly(context, path + ".exe");
        }

        static Assembly LoadAssembly(AssemblyLoadContext context, string assemblyPath)
        {
            try
            {
                if (File.Exists(assemblyPath))
                    using (var stream = File.OpenRead(assemblyPath))
                        return context.LoadFromStream(stream);
            }
            catch { }

            return null;
        }

        public static IDisposable SubscribeResolve()
            => new NetCoreAssemblyHelper(Path.GetDirectoryName(typeof(NetCoreAssemblyHelper).GetTypeInfo().Assembly.Location));
    }
}

#endif
