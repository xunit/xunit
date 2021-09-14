using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using Microsoft.CSharp;
using Xunit;

namespace TestUtility
{
    public class MockAssembly : IDisposable
    {
        readonly AssemblyName assemblyName;
        readonly string filename;

        public MockAssembly()
            : this(Path.GetRandomFileName()) { }

        public MockAssembly(string assemblyFileName)
        {
            filename = Path.Combine(BasePath, assemblyFileName + ".dll");

            assemblyName = new AssemblyName();
            assemblyName.Name = Path.GetFileNameWithoutExtension(filename);
            assemblyName.CodeBase = Path.GetDirectoryName(Path.GetFullPath(filename));
        }

        public AssemblyName AssemblyName
        {
            get { return assemblyName; }
        }

        public static string BasePath
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetLocalCodeBase()); }
        }

        public string FileName
        {
            get { return filename; }
        }

        public string XunitDllFilename
        {
            get { return Path.Combine(BasePath, "xunit.dll"); }
        }

        public void Compile(string code,
                            params string[] references)
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
                    errors.Add(string.Format("{0}({1},{2}): error {3}: {4}", error.FileName, error.Line, error.Column, error.ErrorNumber, error.ErrorText));

                throw new InvalidOperationException("Compilation Failed:" + Environment.NewLine + string.Join(Environment.NewLine, errors.ToArray()));
            }
        }

        public void Dispose()
        {
            if (File.Exists(filename))
                File.Delete(filename);
        }

        public XmlNode Run()
        {
            return Run(null);
        }

        public XmlNode Run(string configFile)
        {
            using (ExecutorWrapper wrapper = new ExecutorWrapper(FileName, configFile, false))
            {
                XmlNode result = null;

                wrapper.RunAssembly(node =>
                                    {
                                        if (node.Name == "assembly")
                                            result = node;

                                        return true;
                                    });

                return result;
            }
        }

        public XmlNode RunWithMate()
        {
            var mate = new MultiAssemblyTestEnvironment();
            mate.Load(FileName);

            string xml = mate.Run(mate.EnumerateTestMethods(), new Callback());

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            // ChildNodes[0] == <assemblies>
            // ChildNodes[0].ChildNodes[0] == first <assembly> node
            return doc.ChildNodes[0].ChildNodes[0];
        }

        class Callback : ITestMethodRunnerCallback
        {
            public void AssemblyFinished(TestAssembly testAssembly, int total, int failed, int skipped, double time)
            {
            }

            public void AssemblyStart(TestAssembly testAssembly)
            {
            }

            public bool ClassFailed(TestClass testClass, string exceptionType, string message, string stackTrace)
            {
                return true;
            }

            public void ExceptionThrown(TestAssembly testAssembly, Exception exception)
            {
            }

            public bool TestFinished(TestMethod testMethod)
            {
                return true;
            }

            public bool TestStart(TestMethod testMethod)
            {
                return true;
            }
        }
    }
}
