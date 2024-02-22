#if NETFRAMEWORK

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class CSharpAcceptanceTestV1Assembly : CSharpAcceptanceTestAssembly
{
    public CSharpAcceptanceTestV1Assembly(string basePath = null)
        : base(basePath)
    { }

    protected override IEnumerable<string> GetStandardReferences()
        => base.GetStandardReferences()
               .Concat(new[] { "xunit.dll", "xunit.extensions.dll" });

    public static async Task<CSharpAcceptanceTestV1Assembly> Create(string code, params string[] references)
    {
        var assembly = new CSharpAcceptanceTestV1Assembly();
        await assembly.Compile(new[] { code }, references);
        return assembly;
    }
}

#endif
