#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.FSharp.Compiler.SimpleSourceCodeServices;
using Microsoft.FSharp.Core;

public abstract class FSharpAcceptanceTestAssembly : AcceptanceTestAssembly
{
    protected FSharpAcceptanceTestAssembly(string basePath)
        : base(basePath) { }

    protected override IEnumerable<string> GetStandardReferences()
    {
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var fxDir = $@"{homeDir}\.nuget\packages\microsoft.netframework.referenceassemblies.net452\1.0.2\build\.NETFramework\v4.5.2\";
        var mscorlib = $@"{fxDir}mscorlib.dll";
        var sysRuntime = $@"{fxDir}Facades\System.Runtime.dll";

        return new[] { mscorlib, sysRuntime, "xunit.abstractions.dll" };
    }


    protected override void Compile(string code, string[] references)
    {
        var sourcePath = Path.GetTempFileName() + ".fs";
        File.WriteAllText(sourcePath, code);

        var compilerArgs =
            new[] {
                "fsc",
                "--noframework",
                sourcePath,
                $"--out:{FileName}",
                $"--pdb:{PdbName}",
                $"--lib:\"{BasePath}\"",
                "--debug",
                "--target:library"
            }
            .Concat(GetStandardReferences().Concat(references).Select(r => $"--reference:{r}"))
            .ToArray();

        var compiler = new SimpleSourceCodeServices(FSharpOption<bool>.Some(false));
        var result = compiler.Compile(compilerArgs);
        if (result.Item2 != 0)
        {
            var errors = result.Item1
                               .Select(e => $"{e.FileName}({e.StartLineAlternate},{e.StartColumn}): {(e.Severity.IsError ? "error" : "warning")} {e.ErrorNumber}: {e.Message}");

            throw new InvalidOperationException($"Compilation Failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}

#endif
