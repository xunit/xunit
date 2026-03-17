using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class RegisterResultWriterAttributeGenerator() :
	IDAndTypeGenerator(
		Types.Xunit.Runner.Common.RegisterResultWriterAttribute,
		nameof(Types.Xunit.Runner.Common.RegisterResultWriterAttribute),
		(id, type) => $$"""
			{
				var writer = new {{type}}();
				global::Xunit.Runner.Common.RegisteredRunnerConfig.RegisterConsoleResultWriter("{{id}}", writer);
				global::Xunit.Runner.Common.RegisteredRunnerConfig.RegisterMicrosoftTestingPlatformResultWriter("{{id}}", writer);
			}
			""")
{
	protected override bool ValidateType(
		INamedTypeSymbol type,
		Location? location,
		GeneratorResult result) =>
			EnsureImplementsInterfaces(type, location, result, Types.Xunit.Runner.Common.IConsoleResultWriter, Types.Xunit.Runner.Common.IMicrosoftTestingPlatformResultWriter);
}
