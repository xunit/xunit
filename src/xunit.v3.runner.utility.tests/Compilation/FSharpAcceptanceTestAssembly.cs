#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FSharp.Compiler.CodeAnalysis;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;

public abstract class FSharpAcceptanceTestAssembly : AcceptanceTestAssembly
{
	protected override IEnumerable<string> GetStandardReferences() => Enumerable.Empty<string>();

	protected override async Task Compile(string code, string[] references)
	{
		var sourcePath = Path.GetTempFileName() + ".fs";
		File.WriteAllText(sourcePath, code);

		var compilerArgs =
			new[] {
				"fsc",
				sourcePath,
				$"--out:{FileName}",
				$"--pdb:{PdbName}",
				$"--lib:\"{BasePath}\"",
				"--debug",
				"--target:library"
			}
			.Concat(GetStandardReferences().Concat(references).Select(r => $"--reference:{r}"))
			.ToArray();

		var checker = FSharpChecker.Create(
			FSharpOption<int>.None,
			FSharpOption<bool>.None,
			FSharpOption<bool>.None,
#pragma warning disable CS0618
			FSharpOption<LegacyReferenceResolver>.None,
#pragma warning restore CS0618
			FSharpOption<FSharpFunc<Tuple<string, DateTime>, FSharpOption<Tuple<object, IntPtr, int>>>>.None,
			FSharpOption<bool>.None,
			FSharpOption<bool>.None,
			FSharpOption<bool>.None,
			FSharpOption<bool>.None
		);

		var resultFSharpAsync = checker.Compile(compilerArgs, FSharpOption<string>.None);
		var result = await FSharpAsync.StartAsTask(resultFSharpAsync, FSharpOption<TaskCreationOptions>.None, FSharpOption<CancellationToken>.None);
		if (result.Item2 != 0)
		{
			var errors =
				result
					.Item1
					.Select(e => $"{e.FileName}({e.StartLine},{e.StartColumn}): {(e.Severity.IsError ? "error" : "warning")} {e.ErrorNumber}: {e.Message}");

			throw new InvalidOperationException($"Compilation Failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
		}
	}
}

#endif
