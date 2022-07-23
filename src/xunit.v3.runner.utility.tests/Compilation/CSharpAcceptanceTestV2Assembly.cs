#if NETFRAMEWORK

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class CSharpAcceptanceTestV2Assembly : CSharpAcceptanceTestAssembly
{
	protected override IEnumerable<string> GetStandardReferences() =>
		base
			.GetStandardReferences()
			.Concat(new[] { "xunit.assert.dll", "xunit.core.dll", "xunit.execution.desktop.dll" });

	public static async ValueTask<CSharpAcceptanceTestV2Assembly> Create(
		string code,
		params string[] references)
	{
		var assembly = new CSharpAcceptanceTestV2Assembly();
		await assembly.Compile(new[] { code }, references);
		return assembly;
	}
}

#endif
