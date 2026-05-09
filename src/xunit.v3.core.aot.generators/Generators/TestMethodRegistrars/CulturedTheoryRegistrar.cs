using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Generators;

static class CulturedTheoryRegistrar
{
	public static CodeGenTestMethodRegistration? GetRegistration(
		SemanticModel semanticModel,
		INamedTypeSymbol classSymbol,
		MethodDeclarationSyntax methodDeclaration,
		IMethodSymbol methodSymbol,
		AttributeData attribute)
	{
		Guard.ArgumentNotNull(semanticModel);
		Guard.ArgumentNotNull(classSymbol);
		Guard.ArgumentNotNull(methodDeclaration);
		Guard.ArgumentNotNull(methodSymbol);
		Guard.ArgumentNotNull(attribute);

		if (methodSymbol.IsGenericMethod)
			return null;

		if (methodSymbol.Parameters.FirstOrDefault(p => p.IsParams) is { } paramsParameter)
			return null;

		var details = new TheoryMethodDetails(semanticModel, classSymbol, methodDeclaration, methodSymbol, attribute);
		if (!details.Process())
			return null;

		var cultures =
			details
				.Attribute
				.ConstructorArguments[0]
				.Values
				.Select(v => v.Value as string)
				.WhereNotNull()
				.ToArray();

		if (cultures.Length == 0)
			return null;

		var initValues = new List<string>
		{
			$"Cultures = [{string.Join(", ", cultures.Select(culture => culture.ToCSharp()))}]",
			$"MethodInvokerFactory = {details.MethodInvokerFactory}",
			$"ParameterNames = new string?[] {{ {string.Join(", ", details.ParameterNames.Select(p => p.ToCSharp()))} }}"
		};

		if (details.DisableDiscoveryEnumeration is not null)
			initValues.Add($"DisableDiscoveryEnumeration = {details.DisableDiscoveryEnumeration.ToCSharp()}");
		if (details.DisplayName is not null)
			initValues.Add($"DisplayName = {details.DisplayName.ToCSharp()}");
		if (details.Explicit)
			initValues.Add("Explicit = true");
		if (details.IncludeTestCaseIndex)
			initValues.Add("IncludeTestCaseIndex = true");
		if (details.ParameterDefaultValues is not null)
			initValues.Add($"ParameterDefaultValues = new string?[] {{ {string.Join(", ", details.ParameterDefaultValues.Select(p => p.ToCSharp()))} }}");
		if (details.SkipExceptions.Count != 0)
			initValues.Add($"SkipExceptions = new global::System.Type[] {{ {string.Join(", ", details.SkipExceptions.Select(e => $"typeof({e})"))} }}");
		if (details.SkipReason is not null)
			initValues.Add($"SkipReason = {details.SkipReason.ToCSharp()}");
		if (details.SkipTestWithoutData)
			initValues.Add("SkipTestWithoutData = true");
		if (details.SkipUnless is not null)
			initValues.Add($"SkipUnless = () => {(details.SkipType ?? classSymbol).ToCSharp()}.{details.SkipUnless}");
		if (details.SkipWhen is not null)
			initValues.Add($"SkipWhen = () => {(details.SkipType ?? classSymbol).ToCSharp()}.{details.SkipWhen}");
		if (details.Timeout is not 0)
			initValues.Add($"Timeout = {details.Timeout}");

		return CodeGenTestMethodRegistration.FromTestMethodDetails(details, $"new global::Xunit.v3.CulturedTheoryTestCaseFactory() {{ {string.Join(", ", initValues)} }}");
	}
}
