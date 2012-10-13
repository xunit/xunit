using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CSharp;

namespace TestUtility
{
    public class MockAssembly : IDisposable
    {
        readonly AssemblyName assemblyName;
        readonly string filename;

        public MockAssembly(string code, params string[] references)
        {
            filename = Path.Combine(BasePath, Path.GetRandomFileName() + ".dll");

            assemblyName = new AssemblyName();
            assemblyName.Name = Path.GetFileNameWithoutExtension(filename);
            assemblyName.CodeBase = Path.GetDirectoryName(Path.GetFullPath(filename));

            Compile(code, references);
        }

        public AssemblyName AssemblyName
        {
            get { return assemblyName; }
        }

        public static string BasePath
        {
            get { return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath); }
        }

        public string FileName
        {
            get { return filename; }
        }

        public string XunitDllFilename
        {
            get { return Path.Combine(BasePath, "xunit2.dll"); }
        }

        void Compile(string code, string[] references)
        {
            CompilerParameters parameters = new CompilerParameters();
            parameters.OutputAssembly = filename;
            parameters.IncludeDebugInformation = true;

            parameters.ReferencedAssemblies.Add("mscorlib.dll");
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Data.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.dll");
            parameters.ReferencedAssemblies.Add(XunitDllFilename);

            if (references != null)
                foreach (string reference in references)
                {
                    string localFilename = Path.Combine(BasePath, reference);

                    if (File.Exists(localFilename))
                        parameters.ReferencedAssemblies.Add(localFilename);
                    else
                        parameters.ReferencedAssemblies.Add(reference);
                }

            Dictionary<string, string> compilerOptions = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };
            CSharpCodeProvider provider = new CSharpCodeProvider(compilerOptions);
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

            if (results.Errors.Count != 0)
            {
                List<string> errors = new List<string>();

                foreach (CompilerError error in results.Errors)
                    errors.Add(String.Format("{0}({1},{2}): error {3}: {4}", error.FileName, error.Line, error.Column, error.ErrorNumber, error.ErrorText));

                throw new InvalidOperationException("Compilation Failed:" + Environment.NewLine + string.Join(Environment.NewLine, errors.ToArray()));
            }
        }

        public void Dispose()
        {
            if (File.Exists(filename))
                File.Delete(filename);
        }
    }
}