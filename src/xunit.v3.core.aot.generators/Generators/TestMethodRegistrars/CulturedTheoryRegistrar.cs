using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Generators;

static class CulturedTheoryRegistrar
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

		if (methodSymbol.IsGenericMethod)
		{
			result.Diagnostics.Add(
				Diagnostic.Create(
					DiagnosticDescriptors.X9010_TheoryMethodCannotBeGeneric,
					methodSymbol.Locations.FirstOrDefault()
				)
			);
			return null;
		}

		if (methodSymbol.Parameters.FirstOrDefault(p => p.IsParams) is { } paramsParameter)
		{
			result.Diagnostics.Add(
				Diagnostic.Create(
					DiagnosticDescriptors.X9011_TheoryParameterCannotBeParams,
					paramsParameter.Locations.FirstOrDefault()
				)
			);
			return null;
		}

		var details = new TheoryMethodDetails(classSymbol, methodDeclaration, methodSymbol, attribute);
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

		var initValues = new List<string>
		{
			$"Cultures = [{string.Join(", ", cultures.Select(culture => culture.Quoted()))}]",
			$"MethodInvokerFactory = {details.MethodInvokerFactory}",
			$"ParameterNames = new string?[] {{ {string.Join(", ", details.ParameterNames.Select(p => p.Quoted()))} }}"
		};

		if (details.DisableDiscoveryEnumeration is not null)
			initValues.Add($"DisableDiscoveryEnumeration = {details.DisableDiscoveryEnumeration.ToCSharp()}");
		if (details.DisplayName is not null)
			initValues.Add($"DisplayName = {details.DisplayName.Quoted()}");
		if (details.Explicit)
			initValues.Add("Explicit = true");
		if (details.ParameterDefaultValues is not null)
			initValues.Add($"ParameterDefaultValues = new string?[] {{ {string.Join(", ", details.ParameterDefaultValues.Select(p => p.Quoted()))} }}");
		if (details.SkipExceptions.Count != 0)
			initValues.Add($"SkipExceptions = new global::System.Type[] {{ {string.Join(", ", details.SkipExceptions.Select(e => $"typeof({e})"))} }}");
		if (details.SkipReason is not null)
			initValues.Add($"SkipReason = {details.SkipReason.Quoted()}");
		if (details.SkipTestWithoutData)
			initValues.Add("SkipTestWithoutData = true");
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
			$"new global::Xunit.v3.CulturedTheoryTestCaseFactory() {{ {string.Join(", ", initValues)} }}"
		);
	}
}
