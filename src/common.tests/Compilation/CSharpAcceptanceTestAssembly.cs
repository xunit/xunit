#if NETFRAMEWORK

using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CSharp;

public abstract class CSharpAcceptanceTestAssembly(string? basePath = null) :
	AcceptanceTestAssembly(basePath)
{
	protected override ValueTask Compile(
		string[] code,
		params string[] references)
	{
		var parameters = GetCompilerParameters();

		parameters.ReferencedAssemblies.AddRange(
			GetStandardReferences()
				.Concat(references ?? [])
				.Select(ResolveReference)
				.ToArray()
		);

		code = code.Concat(GetAdditionalCode()).ToArray();

		var provider = new CSharpCodeProvider();
		var results = provider.CompileAssemblyFromSource(parameters, code);

		if (results.Errors.Count != 0)
		{
			var errors =
				results
					.Errors
					.Cast<CompilerError>()
					.Where(e => e != null)
					.Select(e => $"{e.FileName}({e.Line},{e.Column}): error {e.ErrorNumber}: {e.ErrorText}");

			throw new InvalidOperationException($"Compilation Failed: (BasePath = '{BasePath}', TargetFrameworkReferencePath = '{TargetFrameworkReferencePath}'){Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
		}

		return default;
	}

	protected virtual CompilerParameters GetCompilerParameters() =>
		new()
		{
			IncludeDebugInformation = true,
			OutputAssembly = FileName,
			TreatWarningsAsErrors = false,
			WarningLevel = 0,
		};
}

#endif
