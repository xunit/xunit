#if NET45

using System.Collections.Generic;
using System.Linq;

public class CSharpAcceptanceTestV1Assembly : CSharpAcceptanceTestAssembly
{
    private CSharpAcceptanceTestV1Assembly() { }

    protected override IEnumerable<string> GetStandardReferences()
    {
        return base.GetStandardReferences()
                   .Concat(new[] { "xunit.dll", "xunit.extensions.dll" });
    }

    public static CSharpAcceptanceTestV1Assembly Create(string code, params string[] references)
    {
        var assembly = new CSharpAcceptanceTestV1Assembly();
        assembly.Compile(code, references);
        return assembly;
    }
}

#endif
