#if NETFRAMEWORK

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;

public class CSharpDotNetFrameworkExecutable(string? basePath = null) :
	CSharpAcceptanceTestAssembly(basePath)
{
	protected override string AssemblyFileExtension => ".exe";

	protected override IEnumerable<string> GetAdditionalCode() =>
		base
			.GetAdditionalCode()
			.Append("[assembly: System.Runtime.Versioning.TargetFrameworkAttribute(\".NETFramework,Version=v4.7.2\")]");

	protected override CompilerParameters GetCompilerParameters()
	{
		var result = base.GetCompilerParameters();
		result.GenerateExecutable = true;
		return result;
	}

	// The version here must match the PackageDownload in $/src/Directory.Build.props
	protected override string[] GetTargetFrameworkReferencePaths(string nuGetPackageCachePath) =>
		[
			Path.Combine(nuGetPackageCachePath, "microsoft.netframework.referenceassemblies.net472", "1.0.3", "build", ".NETFramework", "v4.7.2"),
			Path.Combine(nuGetPackageCachePath, "microsoft.netframework.referenceassemblies.net472", "1.0.3", "build", ".NETFramework", "v4.7.2", "Facades"),
		];

	public static ValueTask<CSharpDotNetFrameworkExecutable> Create(
		string code,
		params string[] references) =>
			CreateIn(Path.GetDirectoryName(typeof(CSharpDotNetFrameworkExecutable).Assembly.GetLocalCodeBase())!, code, references);

	public static async ValueTask<CSharpDotNetFrameworkExecutable> CreateIn(
		string basePath,
		string code,
		params string[] references)
	{
		Guard.ArgumentNotNull(basePath);
		Guard.ArgumentNotNull(code);
		Guard.ArgumentNotNull(references);

		basePath = Path.GetFullPath(basePath);
		Guard.ArgumentValid(() => $"Base path '{basePath}' does not exist", Directory.Exists(basePath), nameof(basePath));

		var assembly = new CSharpDotNetFrameworkExecutable(basePath);
		await assembly.Compile([code], references);
		return assembly;
	}
}

#endif
