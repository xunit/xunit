#if NETFRAMEWORK

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CSharp;
using Xunit.Internal;

public abstract class CSharpAcceptanceTestAssembly : AcceptanceTestAssembly
{
	protected CSharpAcceptanceTestAssembly(string? basePath = null) :
		base(basePath)
	{ }

	protected override Task Compile(
		string[] code,
		params string[] references)
	{
		var parameters = new CompilerParameters()
		{
			OutputAssembly = FileName,
			IncludeDebugInformation = true
		};

		parameters.ReferencedAssemblies.AddRange(
			GetStandardReferences()
				.Concat(references ?? new string[0])
				.Select(ResolveReference)
				.ToArray()
		);

		var compilerOptions = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };
		var provider = new CSharpCodeProvider(compilerOptions);
		var results = provider.CompileAssemblyFromSource(parameters, code);

		if (results.Errors.Count != 0)
		{
			var errors = new List<string>();

			foreach (var error in results.Errors.Cast<CompilerError>().WhereNotNull())
				errors.Add($"{error.FileName}({error.Line},{error.Column}): error {error.ErrorNumber}: {error.ErrorText}");

			throw new InvalidOperationException($"Compilation Failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors.ToArray())}");
		}

		return Task.CompletedTask;
	}
}

#endif
