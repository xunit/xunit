#if NETFRAMEWORK

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

public class CSharpAcceptanceTestV1Assembly : CSharpAcceptanceTestAssembly
{
	CSharpAcceptanceTestV1Assembly(string? basePath)
		: base(basePath)
	{ }

	protected override IEnumerable<string> GetStandardReferences() =>
		base
			.GetStandardReferences()
			.Concat(new[] { "xunit.dll", "xunit.extensions.dll" });

	public static async Task<CSharpAcceptanceTestV1Assembly> Create(string code, params string[] references)
	{
		var basePath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
		var assembly = new CSharpAcceptanceTestV1Assembly(basePath);
		await assembly.Compile(code, references);
		return assembly;
	}
}

#endif
