#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Internal;

public abstract class AcceptanceTestAssembly : IDisposable
{
	protected AcceptanceTestAssembly(string? basePath = null)
	{
		BasePath = basePath ?? Path.GetDirectoryName(typeof(AcceptanceTestAssembly).Assembly.GetLocalCodeBase())!;
		FileName = Path.Combine(BasePath, Path.GetRandomFileName() + AssemblyFileExtension);
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

	protected abstract Task Compile(string code, string[] references);

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
