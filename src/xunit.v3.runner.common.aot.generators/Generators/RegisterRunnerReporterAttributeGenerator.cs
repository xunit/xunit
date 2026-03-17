using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class RegisterRunnerReporterAttributeGenerator() :
	XunitAttributeGenerator<RegisterRunnerReporterAttributeGenerator.GeneratorResult>(Types.Xunit.Runner.Common.RegisterRunnerReporterAttribute)
{
	protected override void CreateSource(
		SourceProductionContext context,
		GeneratorResult result)
	{
		if (result is null || result.RunnerReporters.Count == 0)
			return;

		AddInitAttribute(
			context, result,
			string.Join(
				"\r\n",
				result
					.RunnerReporters
					.WhereNotNull()
					.Select(type => $"global::Xunit.Runner.Common.RegisteredRunnerConfig.RegisterRunnerReporter(new {type}());")
			)
		);
	}

	protected override GeneratorResult? Transform(
		GeneratorAttributeSyntaxContext context,
		CancellationToken cancellationToken)
	{
		if (context.TargetSymbol is not IAssemblySymbol)
			return null;

		var result = new GeneratorResult(context);

		foreach (var attribute in context.Attributes)
			if (attribute.ConstructorArguments.Length == 1 &&
				attribute.ConstructorArguments[0].Value is INamedTypeSymbol reporterType)
			{
				var location = attribute.ApplicationSyntaxReference.Location;
				if (EnsureParameterlessPublicCtor(reporterType, location, result, out var _) &&
					EnsureImplementsInterface(reporterType, location, result, Types.Xunit.Runner.Common.IRunnerReporter))
					result.RunnerReporters.Add(reporterType.ToString());
			}

		return result;
	}

	public sealed class GeneratorResult(GeneratorAttributeSyntaxContext context) :
		XunitGeneratorResult(context.SemanticModel, context.TargetNode)
	{
		public List<string?> RunnerReporters = [];
	}
}
