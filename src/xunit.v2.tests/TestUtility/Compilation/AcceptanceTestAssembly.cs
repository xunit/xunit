#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public abstract class AcceptanceTestAssembly : IDisposable
{
    protected AcceptanceTestAssembly(string basePath)
    {
        BasePath = basePath ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetLocalCodeBase());
        FileName = Path.Combine(BasePath, Path.GetRandomFileName() + ".dll");
        PdbName = Path.Combine(BasePath, Path.GetFileNameWithoutExtension(FileName) + ".pdb");

        AssemblyName = new AssemblyName()
        {
            Name = Path.GetFileNameWithoutExtension(FileName),
            CodeBase = Path.GetDirectoryName(Path.GetFullPath(FileName))
        };
    }

    public AssemblyName AssemblyName { get; protected set; }

    public string BasePath { get; }

    public string FileName { get; protected set; }

    public string PdbName { get; protected set; }

    public virtual void Dispose()
    {
        if (File.Exists(FileName))
            File.Delete(FileName);

        if (File.Exists(PdbName))
            File.Delete(PdbName);
    }

    protected abstract void Compile(string code, string[] references);

    protected virtual IEnumerable<string> GetStandardReferences()
        => new[] {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll",
            "System.Data.dll",
            "System.Runtime.dll",
            "System.Xml.dll",
        };

    protected string ResolveReference(string reference)
    {
        var localFilename = Path.Combine(BasePath, reference);
        return File.Exists(localFilename) ? localFilename : reference;
    }
}

#endif
