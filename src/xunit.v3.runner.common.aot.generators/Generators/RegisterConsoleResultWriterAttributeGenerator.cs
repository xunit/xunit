using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class RegisterResultConsoleWriterAttributeGenerator() :
	IDAndTypeGenerator(
		Types.Xunit.Runner.Common.RegisterConsoleResultWriterAttribute,
		nameof(Types.Xunit.Runner.Common.RegisterConsoleResultWriterAttribute),
		(id, type) => $@"global::Xunit.Runner.Common.RegisteredRunnerConfig.RegisterConsoleResultWriter(""{id}"", new {type}());")
{
	protected override bool ValidateType(
		INamedTypeSymbol type,
		Location? location,
		GeneratorResult result) =>
			EnsureImplementsInterface(type, location, result, Types.Xunit.Runner.Common.IConsoleResultWriter);
}
