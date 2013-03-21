using System;
using System.IO;
using System.Reflection;

namespace Xunit.Runner.MSBuild
{
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