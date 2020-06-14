#if NETFRAMEWORK

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp;

public abstract class CSharpAcceptanceTestAssembly : AcceptanceTestAssembly
{
    protected CSharpAcceptanceTestAssembly(string basePath)
        : base(basePath) { }

    protected override void Compile(string code, string[] references)
    {
        var parameters = new CompilerParameters()
        {
            OutputAssembly = FileName,
            IncludeDebugInformation = true
        };

        parameters.ReferencedAssemblies.AddRange(
            GetStandardReferences().Concat(references ?? new string[0])
                                   .Select(ResolveReference)
                                   .ToArray());

        var compilerOptions = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };
        var provider = new CSharpCodeProvider(compilerOptions);
        var results = provider.CompileAssemblyFromSource(parameters, code);

        if (results.Errors.Count != 0)
        {
            var errors = new List<string>();

            foreach (CompilerError error in results.Errors)
                errors.Add($"{error.FileName}({error.Line},{error.Column}): error {error.ErrorNumber}: {error.ErrorText}");

            throw new InvalidOperationException($"Compilation Failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors.ToArray())}");
        }
    }
}

#endif
