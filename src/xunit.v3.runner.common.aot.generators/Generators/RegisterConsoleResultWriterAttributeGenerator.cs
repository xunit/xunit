using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class RegisterConsoleResultWriterAttributeGenerator() :
	IDAndTypeGenerator(
		Types.Xunit.Runner.Common.RegisterConsoleResultWriterAttribute,
		(id, type) => $@"global::Xunit.Runner.Common.RegisteredRunnerConfig.RegisterConsoleResultWriter(""{id}"", new {type}());")
{
	protected override bool ValidateType(
		INamedTypeSymbol type) =>
			type.ImplementsInterface(Types.Xunit.Runner.Common.IConsoleResultWriter);
}

[Generator(LanguageNames.CSharp)]
public class RegisterConsoleResultWriterAttributeOfTGenerator() :
	IDAndTypeGenerator(
		Types.Xunit.Runner.Common.RegisterConsoleResultWriterAttribute + "`1",
		(id, type) => $@"global::Xunit.Runner.Common.RegisteredRunnerConfig.RegisterConsoleResultWriter(""{id}"", new {type}());")
{ }
