using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

public class CSharpAcceptanceTestV2Assembly : CSharpAcceptanceTestAssembly
{
	CSharpAcceptanceTestV2Assembly(string? basePath)
		: base(basePath)
	{ }

	protected override IEnumerable<string> GetStandardReferences() =>
		base
			.GetStandardReferences()
			.Concat(new[] { "xunit.assert.dll", "xunit.core.dll", "xunit.execution.dotnet.dll" });

	public static async Task<CSharpAcceptanceTestV2Assembly> Create(string code, params string[] references)
	{
		var basePath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
		var assembly = new CSharpAcceptanceTestV2Assembly(basePath);
		await assembly.Compile(code, references);
		return assembly;
	}
}
