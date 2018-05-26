#if NETFRAMEWORK

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public class FSharpAcceptanceTestV2Assembly : FSharpAcceptanceTestAssembly
{
    private FSharpAcceptanceTestV2Assembly(string basePath)
        : base(basePath) { }

    protected override IEnumerable<string> GetStandardReferences()
    {
        return base.GetStandardReferences()
                   .Concat(new[] { "xunit.assert.dll", "xunit.core.dll", "xunit.execution.desktop.dll" });
    }

    public static FSharpAcceptanceTestV2Assembly Create(string code, params string[] references)
    {
        var basePath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
        var assembly = new FSharpAcceptanceTestV2Assembly(basePath);
        assembly.Compile(code, references);
        return assembly;
    }
}

#endif
