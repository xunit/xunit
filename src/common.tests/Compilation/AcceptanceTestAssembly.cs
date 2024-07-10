using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit.Internal;

public abstract class AcceptanceTestAssembly :
	IDisposable
{
	string? targetFrameworkReferencePath;

	protected AcceptanceTestAssembly(string? basePath = null)
	{
		BasePath = basePath ?? Path.GetDirectoryName(typeof(AcceptanceTestAssembly).Assembly.GetLocalCodeBase()) ?? throw new InvalidOperationException($"Cannot find local code base for of {typeof(AcceptanceTestAssembly).Assembly.FullName}");
		FileName = Path.Combine(BasePath, Path.GetRandomFileName() + AssemblyFileExtension);
		PdbName = Path.Combine(BasePath, Path.GetFileNameWithoutExtension(FileName) + ".pdb");
	}

	protected virtual string AssemblyFileExtension => ".dll";

	public string BasePath { get; }

	public string FileName { get; protected set; }

	public string PdbName { get; protected set; }

	public string TargetFrameworkReferencePath
	{
		get
		{
			if (targetFrameworkReferencePath is null)
			{
				var nuGetPackageCachePath =
					Environment.GetEnvironmentVariable("NUGET_PACKAGES") ??
					Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");

				targetFrameworkReferencePath = GetTargetFrameworkReferencePath(nuGetPackageCachePath);
			}

			return targetFrameworkReferencePath;
		}
	}

	public virtual void Dispose()
	{
		GC.SuppressFinalize(this);

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

	protected abstract ValueTask Compile(
		string[] code,
		params string[] references);

	protected virtual IEnumerable<string> GetAdditionalCode() => [];

	protected virtual IEnumerable<string> GetStandardReferences() =>
	[
		"System.dll",
		"System.Core.dll",
		"System.Data.dll",
		"System.Runtime.dll",
		"System.Xml.dll",
	];

	// The version here must match the PackageDownload in $/src/Directory.Build.props
	protected virtual string GetTargetFrameworkReferencePath(string nuGetPackageCachePath) =>
		Path.Combine(nuGetPackageCachePath, "netstandard.library", "2.0.0", "build", "netstandard2.0", "ref");

	protected string ResolveReference(string reference) =>
		ResolveReferenceFrom(reference, BasePath) ??
		ResolveReferenceFrom(reference, TargetFrameworkReferencePath) ??
		reference;

	protected string? ResolveReferenceFrom(
		string reference,
		string path)
	{
		var result = Path.Combine(path, reference);
		return File.Exists(result) ? result : null;
	}
}
