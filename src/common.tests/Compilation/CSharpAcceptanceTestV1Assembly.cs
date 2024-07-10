#if NETFRAMEWORK

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;

public class CSharpAcceptanceTestV1Assembly(string? basePath = null) :
	CSharpAcceptanceTestAssembly(basePath)
{
	protected override IEnumerable<string> GetStandardReferences() =>
		base
			.GetStandardReferences()
			.Concat([
				"xunit.dll",
				"xunit.extensions.dll",
			]);

	public static ValueTask<CSharpAcceptanceTestV1Assembly> Create(
		string code,
		params string[] references) =>
			CreateIn(Path.GetDirectoryName(typeof(CSharpAcceptanceTestV1Assembly).Assembly.GetLocalCodeBase())!, code, references);

	public static async ValueTask<CSharpAcceptanceTestV1Assembly> CreateIn(
		string basePath,
		string code,
		params string[] references)
	{
		Guard.ArgumentNotNull(basePath);
		Guard.ArgumentNotNull(code);
		Guard.ArgumentNotNull(references);

		basePath = Path.GetFullPath(basePath);
		Guard.ArgumentValid(() => $"Base path '{basePath}' does not exist", Directory.Exists(basePath), nameof(basePath));

		var assembly = new CSharpAcceptanceTestV1Assembly(basePath);
		await assembly.Compile([code], references);
		return assembly;
	}
}

#endif
