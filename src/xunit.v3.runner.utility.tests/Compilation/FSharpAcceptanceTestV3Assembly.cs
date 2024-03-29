#if NETFRAMEWORK

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Internal;

public class FSharpAcceptanceTestV3Assembly : FSharpAcceptanceTestAssembly
{
	public FSharpAcceptanceTestV3Assembly(string? basePath = null) :
		base(basePath)
	{ }

	protected override string AssemblyFileExtension => ".exe";

	protected override IEnumerable<string> GetStandardReferences() =>
		base
			.GetStandardReferences()
			.Concat([
				"System.Threading.Tasks.Extensions.dll",
				"xunit.v3.assert.dll",
				"xunit.v3.common.dll",
				"xunit.v3.core.dll",
				"xunit.v3.runner.common.dll",
				"xunit.v3.runner.inproc.console.dll",
			]);

	public static async Task<FSharpAcceptanceTestV3Assembly> Create(
		string code,
		params string[] references)
	{
		var testFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetLocalCodeBase());
		Assert.NotNull(testFolder);

		var programPath = Path.Combine(testFolder, "..", "..", "..", "..", "xunit.v3.core", "Package", "content", "AutoGeneratedEntryPoint.fs");
		programPath = Path.GetFullPath(programPath);
		Assert.True(File.Exists(programPath), $"Cannot find '{programPath}' to include into compilation");

		var programText = File.ReadAllText(programPath);

		var assembly = new FSharpAcceptanceTestV3Assembly();
		await assembly.Compile([code, programText], references);
		return assembly;
	}
}

#endif
