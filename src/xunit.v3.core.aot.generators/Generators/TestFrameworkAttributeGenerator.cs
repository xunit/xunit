using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class TestFrameworkAttributeGenerator() :
	AssemblyFactoryAttributeGeneratorBase(Types.Xunit.TestFrameworkAttribute, nameof(Types.Xunit.TestFrameworkAttribute), "RegisterTestFrameworkFactory")
{
	static readonly HashSet<string> StringTypes = ["string", "string?"];

	protected override string? GetFactory(
		INamedTypeSymbol type,
		Location? location,
		GeneratorResult result)
	{
		Guard.ArgumentNotNull(type);
		Guard.ArgumentNotNull(result);

		// First check for a ctor that takes a string/string?
		var ctor = type.Constructors.FirstOrDefault(c =>
			!c.IsStatic
				&& c.DeclaredAccessibility == Accessibility.Public
				&& c.Parameters.Length == 1
				&& StringTypes.Contains(c.Parameters[0].Type.ToCSharp())
		);
		if (ctor is not null)
			return $"configFileName => new {type.ToCSharp()}(configFileName)";

		// Fall back to a parameterless ctor
		ctor = type.Constructors.FirstOrDefault(c => !c.IsStatic && c.DeclaredAccessibility == Accessibility.Public && c.Parameters.Length == 0);
		if (ctor is not null)
			return $"configFileName => new {type.ToCSharp()}()";

		result.Diagnostics.Add(
			Diagnostic.Create(
				DiagnosticDescriptors.X9000_TypeMustHaveCorrectPublicConstructor,
				location,
				type.ToDisplayString(),
				"string? configFileName"  // Encourage them to take the config file
			)
		);

		return null;
	}

	protected override bool ValidateImplementationType(
		INamedTypeSymbol type,
		Location? location,
		GeneratorResult result) =>
			EnsureImplementsInterface(type, location, result, Types.Xunit.v3.ITestFramework);
}
