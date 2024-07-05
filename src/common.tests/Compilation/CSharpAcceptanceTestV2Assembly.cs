#if NETFRAMEWORK

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class CSharpAcceptanceTestV2Assembly : CSharpAcceptanceTestAssembly
{
	public CSharpAcceptanceTestV2Assembly(string? basePath = null) :
		base(basePath)
	{ }

	protected override IEnumerable<string> GetStandardReferences() =>
		base
			.GetStandardReferences()
			.Concat([
				"xunit.assert.dll",
				"xunit.core.dll",
				"xunit.execution.desktop.dll",
			]);

	public static async Task<CSharpAcceptanceTestV2Assembly> Create(
		string code,
		params string[] references)
	{
		var assembly = new CSharpAcceptanceTestV2Assembly();
		await assembly.Compile([code], references);
		return assembly;
	}
}

#endif
