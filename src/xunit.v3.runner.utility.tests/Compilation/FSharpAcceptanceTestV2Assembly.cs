#if NETFRAMEWORK

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;

public class FSharpAcceptanceTestV2Assembly(string? basePath = null) :
	FSharpAcceptanceTestAssembly(basePath)
{
	protected override IEnumerable<string> GetStandardReferences() =>
		base
			.GetStandardReferences()
			.Concat([
				"xunit.assert.dll",
				"xunit.core.dll",
				"xunit.execution.desktop.dll",
			]);

	public static ValueTask<FSharpAcceptanceTestV2Assembly> Create(
		string code,
		params string[] references) =>
			CreateIn(Path.GetDirectoryName(typeof(FSharpAcceptanceTestV2Assembly).Assembly.GetLocalCodeBase())!, code, references);

	public static async ValueTask<FSharpAcceptanceTestV2Assembly> CreateIn(
		string basePath,
		string code,
		params string[] references)
	{
		Guard.ArgumentNotNull(basePath);
		Guard.ArgumentNotNull(code);
		Guard.ArgumentNotNull(references);

		basePath = Path.GetFullPath(basePath);
		Guard.ArgumentValid(() => $"Base path '{basePath}' does not exist", Directory.Exists(basePath), nameof(basePath));

		var assembly = new FSharpAcceptanceTestV2Assembly(basePath);
		await assembly.Compile([code], references);
		return assembly;
	}
}

#endif
