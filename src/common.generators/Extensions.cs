using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

partial class Extensions
{
	public static ImmutableArray<ISymbol> GetAllMembers(
		this INamedTypeSymbol type,
		string name)
	{
		var result = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

		for (var current = type; current is not null; current = current.BaseType)
			foreach (var member in current.GetMembers(name))
				result.Add(member);

		foreach (var methodWithOverride in result.OfType<IMethodSymbol>().Where(m => m.IsOverride).ToArray())
			if (methodWithOverride.OverriddenMethod is not null)
				result.Remove(methodWithOverride.OverriddenMethod);

		return result.ToImmutableArray();
	}

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

	public static bool IsGeneric(
		this INamedTypeSymbol? type,
		string genericTypeName) =>
			type is not null && type.IsGenericType && type.ConstructUnboundGenericType().ToCSharp(includeGlobal: false) == genericTypeName;
}
