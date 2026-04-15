using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Generators;

static class FactRegistrar
{
	public static FactMethodRegistration? GetRegistration(
		INamedTypeSymbol classSymbol,
		MethodDeclarationSyntax methodDeclaration,
		IMethodSymbol methodSymbol,
		AttributeData attribute)
	{
		Guard.ArgumentNotNull(classSymbol);
		Guard.ArgumentNotNull(methodDeclaration);
		Guard.ArgumentNotNull(methodSymbol);
		Guard.ArgumentNotNull(attribute);

		var details = new FactMethodDetails(classSymbol, methodDeclaration, methodSymbol, attribute);
		if (!details.Process())
			return null;

		var initValues = new List<string>
		{
			$"MethodInvoker = {details.MethodInvoker}"
		};

		if (details.DisplayName is not null)
			initValues.Add($"DisplayName = {details.DisplayName.Quoted()}");
		if (details.Explicit)
			initValues.Add("Explicit = true");
		if (details.SkipExceptions.Count != 0)
			initValues.Add($"SkipExceptions = new global::System.Type[] {{ {string.Join(", ", details.SkipExceptions.Select(e => $"typeof({e})"))} }}");
		if (details.SkipReason is not null)
			initValues.Add($"SkipReason = {details.SkipReason.Quoted()}");
		if (details.SkipUnless is not null)
			initValues.Add($"SkipUnless = () => {(details.SkipType ?? classSymbol).ToCSharp()}.{details.SkipUnless}");
		if (details.SkipWhen is not null)
			initValues.Add($"SkipWhen = () => {(details.SkipType ?? classSymbol).ToCSharp()}.{details.SkipWhen}");
		if (details.Timeout is not 0)
			initValues.Add($"Timeout = {details.Timeout}");

		return new(
			details.MethodName,
			new()
			{
				Arity = details.Arity,
				BeforeAfterAttributeTypes = details.BeforeAfterTestAttributes.Count != 0 ? details.BeforeAfterTestAttributes : null,
				DeclaredTypeIndex = details.DeclaredTypeIndex,
				IsStatic = details.MethodIsStatic,
				SourceFilePath = details.SourceFilePath,
				SourceLineNumber = details.SourceLineNumber,
				TestCaseOrdererFactory = details.TestCaseOrdererFactory,
				Traits = details.Traits,
			},
			$"new global::Xunit.v3.FactTestCaseFactory() {{ {string.Join(", ", initValues)} }}"
		);
	}
}
