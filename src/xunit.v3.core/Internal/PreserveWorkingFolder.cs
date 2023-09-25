using System;
using System.IO;
using Xunit.v3;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public class PreserveWorkingFolder : IDisposable
{
	readonly string originalWorkingFolder;

	/// <summary/>
	public PreserveWorkingFolder(_IAssemblyInfo assemblyInfo)
	{
		Guard.ArgumentNotNull(assemblyInfo);

		originalWorkingFolder = Directory.GetCurrentDirectory();

		if (!string.IsNullOrWhiteSpace(assemblyInfo.AssemblyPath))
		{
			var assemblyFolder = Path.GetDirectoryName(assemblyInfo.AssemblyPath);
			if (!string.IsNullOrWhiteSpace(assemblyFolder))
				Directory.SetCurrentDirectory(assemblyFolder);
		}
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
