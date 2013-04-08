using System;
using System.IO;
using System.Reflection;

namespace Xunit
{
    /// <summary>
    /// This class provides assistance with assembly resolution for missing assemblies. Runners may
    /// need to use <see cref="SubscribeResolve" /> to help automatically resolve missing assemblies
    /// when running tests.
    /// </summary>
    public static class AssemblyHelper
    {
        static string loadPath = Path.GetDirectoryName(new Uri(typeof(AssemblyHelper).Assembly.CodeBase).LocalPath);

        static Assembly LoadAssembly(AssemblyName assemblyName)
        {
            string path = Path.Combine(loadPath, assemblyName.Name);
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

        static Assembly Resolve(object sender, ResolveEventArgs args)
        {
            return LoadAssembly(new AssemblyName(args.Name));
        }

        /// <summary>
        /// Subscribes to the current <see cref="AppDomain"/> <see cref="AppDomain.AssemblyResolve"/> event, to
        /// provide automatic assembly resolution for assemblies in the runner.
        /// </summary>
        /// <returns>IDisposable.</returns>
        public static IDisposable SubscribeResolve()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.Resolve;
            return new ResolveUnsubscribe();
        }

        class ResolveUnsubscribe : IDisposable
        {
            public void Dispose()
            {
                AppDomain.CurrentDomain.AssemblyResolve -= AssemblyHelper.Resolve;
            }
        }
    }
}