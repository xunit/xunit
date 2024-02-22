#if NETFRAMEWORK

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class FSharpAcceptanceTestV2Assembly : FSharpAcceptanceTestAssembly
{
    public FSharpAcceptanceTestV2Assembly(string basePath = null)
        : base(basePath)
    { }

    protected override IEnumerable<string> GetStandardReferences() =>
        base
            .GetStandardReferences()
            .Concat(new[] { "xunit.assert.dll", "xunit.core.dll", "xunit.execution.desktop.dll" });

    public static async Task<FSharpAcceptanceTestV2Assembly> Create(string code, params string[] references)
    {
        var assembly = new FSharpAcceptanceTestV2Assembly();
        await assembly.Compile(new[] { code }, references);
        return assembly;
    }
}

#endif
