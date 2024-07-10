#if NETFRAMEWORK

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;

public class CSharpAcceptanceTestV2Assembly(string? basePath = null) :
	CSharpAcceptanceTestAssembly(basePath)
{
	protected override IEnumerable<string> GetStandardReferences() =>
		base
			.GetStandardReferences()
			.Concat([
				"xunit.assert.dll",
				"xunit.core.dll",
				"xunit.execution.desktop.dll",
			]);

	public static ValueTask<CSharpAcceptanceTestV2Assembly> Create(
		string code,
		params string[] references) =>
			CreateIn(Path.GetDirectoryName(typeof(CSharpAcceptanceTestV2Assembly).Assembly.GetLocalCodeBase())!, code, references);

	public static async ValueTask<CSharpAcceptanceTestV2Assembly> CreateIn(
		string basePath,
		string code,
		params string[] references)
	{
		Guard.ArgumentNotNull(basePath);
		Guard.ArgumentNotNull(code);
		Guard.ArgumentNotNull(references);

		basePath = Path.GetFullPath(basePath);
		Guard.ArgumentValid(() => $"Base path '{basePath}' does not exist", Directory.Exists(basePath), nameof(basePath));

		var assembly = new CSharpAcceptanceTestV2Assembly(basePath);
		await assembly.Compile([code], references);
		return assembly;
	}
}

#endif
