#if NET45

using System.Collections.Generic;
using System.Linq;

public class FSharpAcceptanceTestV2Assembly : FSharpAcceptanceTestAssembly
{
    private FSharpAcceptanceTestV2Assembly() { }

    protected override IEnumerable<string> GetStandardReferences()
    {
        return base.GetStandardReferences()
                   .Concat(new[] { "xunit.assert.dll", "xunit.core.dll", "xunit.execution.desktop.dll" });
    }

    public static FSharpAcceptanceTestV2Assembly Create(string code, params string[] references)
    {
        var assembly = new FSharpAcceptanceTestV2Assembly();
        assembly.Compile(code, references);
        return assembly;
    }
}

#endif
