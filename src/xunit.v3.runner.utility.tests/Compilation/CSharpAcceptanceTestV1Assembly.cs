#if NETFRAMEWORK

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class CSharpAcceptanceTestV1Assembly : CSharpAcceptanceTestAssembly
{
	protected override IEnumerable<string> GetStandardReferences() =>
		base
			.GetStandardReferences()
			.Concat(new[] { "xunit.dll", "xunit.extensions.dll" });

	public static async ValueTask<CSharpAcceptanceTestV1Assembly> Create(
		string code,
		params string[] references)
	{
		var assembly = new CSharpAcceptanceTestV1Assembly();
		await assembly.Compile(new[] { code }, references);
		return assembly;
	}
}

#endif
