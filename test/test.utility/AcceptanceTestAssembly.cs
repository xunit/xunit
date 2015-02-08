using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CSharp;
using Xunit;

public class AcceptanceTestAssembly : IDisposable
{
    readonly AssemblyName assemblyName;
    readonly string filename;

    public AcceptanceTestAssembly(string code, params string[] references)
    {
        filename = Path.Combine(BasePath, Path.GetRandomFileName() + ".dll");

        assemblyName = new AssemblyName()
        {
            Name = Path.GetFileNameWithoutExtension(filename),
            CodeBase = Path.GetDirectoryName(Path.GetFullPath(filename))
        };

        Compile(code, references);
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

    public string XunitCoreDllFilename
    {
        get { return Path.Combine(BasePath, "xunit.core.dll"); }
    }

    public string XunitExecutionDllFilename
    {
        get { return Path.Combine(BasePath, ExecutionHelper.AssemblyFileName); }
    }

    void Compile(string code, string[] references)
    {
        var parameters = new CompilerParameters()
        {
            OutputAssembly = filename,
            IncludeDebugInformation = true
        };

        parameters.ReferencedAssemblies.Add("mscorlib.dll");
        parameters.ReferencedAssemblies.Add("System.dll");
        parameters.ReferencedAssemblies.Add("System.Core.dll");
        parameters.ReferencedAssemblies.Add("System.Data.dll");
        parameters.ReferencedAssemblies.Add("System.Runtime.dll");
        parameters.ReferencedAssemblies.Add("System.Xml.dll");
        parameters.ReferencedAssemblies.Add(XunitCoreDllFilename);
        parameters.ReferencedAssemblies.Add(XunitExecutionDllFilename);

        if (references != null)
            foreach (string reference in references)
            {
                string localFilename = Path.Combine(BasePath, reference);

                if (File.Exists(localFilename))
                    parameters.ReferencedAssemblies.Add(localFilename);
                else
                    parameters.ReferencedAssemblies.Add(reference);
            }

        var compilerOptions = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };
        var provider = new CSharpCodeProvider(compilerOptions);
        var results = provider.CompileAssemblyFromSource(parameters, code);

        if (results.Errors.Count != 0)
        {
            var errors = new List<string>();

            foreach (CompilerError error in results.Errors)
                errors.Add(String.Format("{0}({1},{2}): error {3}: {4}", error.FileName, error.Line, error.Column, error.ErrorNumber, error.ErrorText));

            throw new InvalidOperationException(String.Format("Compilation Failed:{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, errors.ToArray())));
        }
    }

    public void Dispose()
    {
        if (File.Exists(filename))
            File.Delete(filename);
    }
}