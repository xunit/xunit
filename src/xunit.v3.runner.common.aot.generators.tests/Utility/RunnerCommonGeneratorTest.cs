using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

public class RunnerCommonGeneratorTest<TGenerator>
	where TGenerator : IIncrementalGenerator, new()
{
	static readonly ImmutableArray<MetadataReference> references;

	static RunnerCommonGeneratorTest()
	{
#if DEBUG
		var configuration = "Debug";
#else
		var configuration = "Release";
#endif

		var solutionFolder = AppContext.BaseDirectory;

		while (!File.Exists(Path.Combine(solutionFolder, "xunit.slnx")))
			solutionFolder = Path.GetDirectoryName(solutionFolder)
				?? throw new InvalidOperationException($"Could not find xunit.slnx anywhere along the path chain of '{AppContext.BaseDirectory}'");

		references = new MetadataReference[] {
			MetadataReference.CreateFromFile(Path.Combine(solutionFolder, "src", "xunit.v3.runner.common.aot", "bin", configuration, "net9.0", "xunit.v3.common.aot.dll")),
			MetadataReference.CreateFromFile(Path.Combine(solutionFolder, "src", "xunit.v3.runner.common.aot", "bin", configuration, "net9.0", "xunit.v3.runner.common.aot.dll")),
		}.Concat(Basic.Reference.Assemblies.Net90.References.All).ToImmutableArray();
	}

	protected static GeneratorRunResult Generate(string source)
	{
		var cancellationToken = TestContext.Current.CancellationToken;

		var compilation = CSharpCompilation.Create(
			"TestProject",
			[CSharpSyntaxTree.ParseText(source, cancellationToken: cancellationToken)],
			references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
		);

		var diagnostics = compilation.GetDiagnostics().Where(d => d.DefaultSeverity != DiagnosticSeverity.Hidden).ToArray();
		if (diagnostics.Length != 0)
			Assert.Fail($"One or more diagnostics were reported during compilation:{Environment.NewLine}{string.Join(Environment.NewLine, diagnostics.Select(d => d.ToString()))}");

		var generator = new TGenerator();
		GeneratorDriver driver = CSharpGeneratorDriver.Create(generators: [generator.AsSourceGenerator()]);
		driver = driver.RunGenerators(compilation, cancellationToken);

		var runResult = driver.GetRunResult();

		diagnostics = runResult.Diagnostics.Where(d => d.DefaultSeverity != DiagnosticSeverity.Hidden).ToArray();
		if (diagnostics.Length != 0)
			Assert.Fail($"One or more diagnostics were reported during generation:{Environment.NewLine}{string.Join(Environment.NewLine, diagnostics.Select(d => d.ToString()))}");

		return Assert.Single(runResult.Results);
	}

	protected static string GenerateSingleSource(string source)
	{
		var generatedSource = Assert.Single(Generate(source).GeneratedSources);
		return generatedSource.SourceText.ToString();
	}
}
