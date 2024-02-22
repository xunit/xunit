#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

public abstract class AcceptanceTestAssembly : IDisposable
{
    protected static readonly Task CompletedTask = Task.FromResult(0);

    protected AcceptanceTestAssembly(string basePath = null)
    {
        var packagesPath =
            Environment.GetEnvironmentVariable("NUGET_PACKAGES") ??
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");

        BasePath = basePath ?? Path.GetDirectoryName(typeof(AcceptanceTestAssembly).Assembly.GetLocalCodeBase());
        FileName = Path.Combine(BasePath, Path.GetRandomFileName() + AssemblyFileExtension);
        NetStandardReferencePath = Path.Combine(packagesPath, "netstandard.library", "2.0.0", "build", "netstandard2.0", "ref");
        PdbName = Path.Combine(BasePath, Path.GetFileNameWithoutExtension(FileName) + ".pdb");

        AssemblyName = new AssemblyName()
        {
            Name = Path.GetFileNameWithoutExtension(FileName),
            CodeBase = Path.GetDirectoryName(Path.GetFullPath(FileName))
        };
    }

    protected virtual string AssemblyFileExtension => ".dll";

    public AssemblyName AssemblyName { get; protected set; }

    public string BasePath { get; }

    public string FileName { get; protected set; }

    public string NetStandardReferencePath { get; }

    public string PdbName { get; protected set; }

    public virtual void Dispose()
    {
        try
        {
            if (File.Exists(FileName))
                File.Delete(FileName);
        }
        catch { }

        try
        {
            if (File.Exists(PdbName))
                File.Delete(PdbName);
        }
        catch { }
    }

    protected abstract Task Compile(string[] code, params string[] references);

    protected virtual IEnumerable<string> GetStandardReferences()
        => new[] { "netstandard.dll", "System.Runtime.dll" };

    protected string ResolveReference(string reference) =>
        ResolveReferenceFrom(reference, BasePath) ??
        ResolveReferenceFrom(reference, NetStandardReferencePath) ??
        reference;

    protected string ResolveReferenceFrom(string reference, string path)
    {
        var result = Path.Combine(path, reference);
        return File.Exists(result) ? result : null;
    }
}

#endif
