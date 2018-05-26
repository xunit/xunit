#if NETFRAMEWORK

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public class CSharpAcceptanceTestV1Assembly : CSharpAcceptanceTestAssembly
{
    private CSharpAcceptanceTestV1Assembly(string basePath)
        : base(basePath) { }

    protected override IEnumerable<string> GetStandardReferences()
        => base.GetStandardReferences()
               .Concat(new[] { "xunit.dll", "xunit.extensions.dll" });

    public static CSharpAcceptanceTestV1Assembly Create(string code, params string[] references)
    {
        var basePath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
        var assembly = new CSharpAcceptanceTestV1Assembly(basePath);
        assembly.Compile(code, references);
        return assembly;
    }
}

#endif
