using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

partial class Extensions
{
	public static bool HasParameterlessPublicCtor(
		this INamedTypeSymbol symbol,
		[NotNullWhen(true)] out IMethodSymbol? ctor)
	{
		ctor =
			symbol
				?.Constructors
				.FirstOrDefault(c => !c.IsStatic && c.DeclaredAccessibility == Accessibility.Public && c.Parameters.All(p => p.IsOptional || p.IsParams));

		return ctor is not null;
	}
}
