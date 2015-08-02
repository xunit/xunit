using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CSharp;

public abstract class AcceptanceTestAssembly : IDisposable
{
    protected AcceptanceTestAssembly()
    {
        FileName = Path.Combine(BasePath, Path.GetRandomFileName() + ".dll");
        PdbName = Path.Combine(BasePath, Path.GetFileNameWithoutExtension(FileName) + ".pdb");

        AssemblyName = new AssemblyName()
        {
            Name = Path.GetFileNameWithoutExtension(FileName),
            CodeBase = Path.GetDirectoryName(Path.GetFullPath(FileName))
        };
    }

    public AssemblyName AssemblyName { get; protected set; }

    public static string BasePath
    {
        get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetLocalCodeBase()); }
    }

    public string FileName { get; protected set; }

    public string PdbName { get; protected set; }

    protected virtual void AddStandardReferences(CompilerParameters parameters)
    {
        parameters.ReferencedAssemblies.Add("mscorlib.dll");
        parameters.ReferencedAssemblies.Add("System.dll");
        parameters.ReferencedAssemblies.Add("System.Core.dll");
        parameters.ReferencedAssemblies.Add("System.Data.dll");
        parameters.ReferencedAssemblies.Add("System.Runtime.dll");
        parameters.ReferencedAssemblies.Add("System.Xml.dll");
    }

    protected void Compile(string code, string[] references)
    {
        var parameters = new CompilerParameters()
        {
            OutputAssembly = FileName,
            IncludeDebugInformation = true
        };

        AddStandardReferences(parameters);

        if (references != null)
            foreach (var reference in references)
            {
                var localFilename = Path.Combine(BasePath, reference);

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
                errors.Add($"{error.FileName}({error.Line},{error.Column}): error {error.ErrorNumber}: {error.ErrorText}");

            throw new InvalidOperationException($"Compilation Failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors.ToArray())}");
        }
    }

    public void Dispose()
    {
        if (File.Exists(FileName))
            File.Delete(FileName);

        if (File.Exists(PdbName))
            File.Delete(PdbName);
    }
}
