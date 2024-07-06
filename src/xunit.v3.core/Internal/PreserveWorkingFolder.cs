using System;
using System.IO;
using Xunit.Sdk;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public class PreserveWorkingFolder : IDisposable
{
	readonly string originalWorkingFolder;

	/// <summary/>
	public PreserveWorkingFolder(ITestAssembly assembly)
	{
		Guard.ArgumentNotNull(assembly);

		originalWorkingFolder = Directory.GetCurrentDirectory();

		var assemblyFolder = Path.GetDirectoryName(assembly.AssemblyPath);
		if (!string.IsNullOrWhiteSpace(assemblyFolder))
			Directory.SetCurrentDirectory(assemblyFolder);
	}

	/// <summary/>
	public void Dispose()
	{
		GC.SuppressFinalize(this);

		try
		{
			if (!string.IsNullOrWhiteSpace(originalWorkingFolder))
				Directory.SetCurrentDirectory(originalWorkingFolder);
		}
		catch { }
	}
}
