#if NETFRAMEWORK

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// TODO: We need program injection here as well
public class FSharpAcceptanceTestV3Assembly : FSharpAcceptanceTestAssembly
{
	protected override string AssemblyFileExtension => ".exe";

	protected override IEnumerable<string> GetStandardReferences() =>
		base
			.GetStandardReferences()
			.Concat(new[] { "xunit.v3.assert.dll", "xunit.v3.core.dll" });

	public static async Task<FSharpAcceptanceTestV3Assembly> Create(
		string code,
		params string[] references)
	{
		var assembly = new FSharpAcceptanceTestV3Assembly();
		await assembly.Compile(code, references);
		return assembly;
	}
}

#endif
