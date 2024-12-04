using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit.Internal;

public abstract class AcceptanceTestAssembly :
	IDisposable
{
	string[]? targetFrameworkReferencePaths;

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

	public string[] TargetFrameworkReferencePaths
	{
		get
		{
			if (targetFrameworkReferencePaths is null)
			{
				var nuGetPackageCachePath =
					Environment.GetEnvironmentVariable("NUGET_PACKAGES") ??
					Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");

				targetFrameworkReferencePaths = GetTargetFrameworkReferencePaths(nuGetPackageCachePath);
			}

			return targetFrameworkReferencePaths;
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
	protected virtual string[] GetTargetFrameworkReferencePaths(string nuGetPackageCachePath) =>
		[Path.Combine(nuGetPackageCachePath, "netstandard.library", "2.0.0", "build", "netstandard2.0", "ref")];

	protected string ResolveReference(string reference) =>
		ResolveReferenceFrom(reference, BasePath) ??
		ResolveReferenceFrom(reference, TargetFrameworkReferencePaths) ??
		reference;

	protected string? ResolveReferenceFrom(
		string reference,
		params string[] paths)
	{
		foreach (var path in paths)
		{
			var result = Path.Combine(path, reference);
			if (File.Exists(result))
				return result;
		}

		return null;
	}
}
