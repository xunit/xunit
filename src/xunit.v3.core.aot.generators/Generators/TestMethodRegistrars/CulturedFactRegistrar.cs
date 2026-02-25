using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Generators;

static class CulturedFactRegistrar
{
	public static FactMethodRegistration? GetRegistration(
		INamedTypeSymbol classSymbol,
		MethodDeclarationSyntax methodDeclaration,
		IMethodSymbol methodSymbol,
		AttributeData attribute,
		TestClassGeneratorResult result)
	{
		Guard.ArgumentNotNull(classSymbol);
		Guard.ArgumentNotNull(methodDeclaration);
		Guard.ArgumentNotNull(methodSymbol);
		Guard.ArgumentNotNull(attribute);
		Guard.ArgumentNotNull(result);

		if (attribute.ConstructorArguments.Length < 1)
			return null;

		var details = new FactMethodDetails(classSymbol, methodDeclaration, methodSymbol, attribute);
		details.Process();

		if (details.Diagnostics.Count != 0)
		{
			result.Diagnostics.AddRange(details.Diagnostics);
			return null;
		}

		var cultures =
			details
				.Attribute
				.ConstructorArguments[0]
				.Values
				.Select(v => v.Value as string)
				.WhereNotNull()
				.ToArray();

		if (cultures.Length == 0)
		{
			result.Diagnostics.Add(
				Diagnostic.Create(
					DiagnosticDescriptors.X9008_CulturedTestMustHaveAtLeastOneCulture,
					details.Attribute.ApplicationSyntaxReference.Location
				)
			);
			return null;
		}

		var testMethod = new CodeGenTestMethodRegistration()
		{
			Arity = details.Arity,
			BeforeAfterAttributeTypes = details.BeforeAfterTestAttributes,
			DeclaredTypeIndex = details.DeclaredTypeIndex,
			IsStatic = details.MethodIsStatic,
			SourceFilePath = details.SourceFilePath,
			SourceLineNumber = details.SourceLineNumber,
			TestCaseOrdererType = details.TestCaseOrderer,
			Traits = details.Traits,
		};

		var initValues = new List<string>
		{
			$"Cultures = [{string.Join(", ", cultures.Select(culture => culture.Quoted()))}]",
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
		if (details.Traits.Count != 0)
			initValues.Add($"Traits = {CodeGenRegistration.ToTraits(details.Traits)}");

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
				TestCaseOrdererType = details.TestCaseOrderer,
				Traits = details.Traits,
			},
			$"new global::Xunit.v3.CulturedFactTestCaseFactory() {{ {string.Join(", ", initValues)} }}"
		);
	}
}
