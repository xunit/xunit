using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Generators;

static class CulturedFactRegistrar
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

		if (attribute.ConstructorArguments.Length < 1)
			return null;

		var details = new FactMethodDetails(semanticModel, classSymbol, methodDeclaration, methodSymbol, attribute);
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
			$"MethodInvoker = {details.MethodInvoker}"
		};

		if (details.DisplayName is not null)
			initValues.Add($"DisplayName = {details.DisplayName.ToCSharp()}");
		if (details.Explicit)
			initValues.Add("Explicit = true");
		if (details.SkipExceptions.Count != 0)
			initValues.Add($"SkipExceptions = new global::System.Type[] {{ {string.Join(", ", details.SkipExceptions.Select(e => $"typeof({e})"))} }}");
		if (details.SkipReason is not null)
			initValues.Add($"SkipReason = {details.SkipReason.ToCSharp()}");
		if (details.SkipUnless is not null)
			initValues.Add($"SkipUnless = () => {(details.SkipType ?? classSymbol).ToCSharp()}.{details.SkipUnless}");
		if (details.SkipWhen is not null)
			initValues.Add($"SkipWhen = () => {(details.SkipType ?? classSymbol).ToCSharp()}.{details.SkipWhen}");
		if (details.Timeout is not 0)
			initValues.Add($"Timeout = {details.Timeout}");

		return CodeGenTestMethodRegistration.FromTestMethodDetails(details, $"new global::Xunit.v3.CulturedFactTestCaseFactory() {{ {string.Join(", ", initValues)} }}");
	}
}
