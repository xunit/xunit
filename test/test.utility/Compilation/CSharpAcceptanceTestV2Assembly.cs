#if NET452

using System.Collections.Generic;
using System.Linq;

public class CSharpAcceptanceTestV2Assembly : CSharpAcceptanceTestAssembly
{
    private CSharpAcceptanceTestV2Assembly() { }

    protected override IEnumerable<string> GetStandardReferences()
    {
        return base.GetStandardReferences()
                   .Concat(new[] { "xunit.assert.dll", "xunit.core.dll", "xunit.execution.desktop.dll" });
    }

    public static CSharpAcceptanceTestV2Assembly Create(string code, params string[] references)
    {
        var assembly = new CSharpAcceptanceTestV2Assembly();
        assembly.Compile(code, references);
        return assembly;
    }
}

#endif
