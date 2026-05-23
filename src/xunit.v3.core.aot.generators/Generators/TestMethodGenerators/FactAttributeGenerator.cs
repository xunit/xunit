using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Generators;

[TestMethodGenerator(Types.Xunit.FactAttribute)]
public class FactAttributeGenerator : ITestMethodGenerator
{
	public CodeGenTestMethodRegistration? GetTestMethodRegistration(
		SemanticModel semanticModel,
		INamedTypeSymbol testClass,
		MethodDeclarationSyntax testMethodSyntax,
		IMethodSymbol testMethod,
		AttributeData attribute)
	{
		Guard.ArgumentNotNull(semanticModel);
		Guard.ArgumentNotNull(testClass);
		Guard.ArgumentNotNull(testMethodSyntax);
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(attribute);

		if (!testMethod.HasValidTestMethodReturnValue())
			return null;

		var details = new FactMethodDetails(semanticModel, testClass, testMethodSyntax, testMethod, attribute);
		if (!details.Process())
			return null;

		var initValues = new List<string>
		{
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
			initValues.Add($"SkipUnless = () => {(details.SkipType ?? testClass).ToCSharp()}.{details.SkipUnless}");
		if (details.SkipWhen is not null)
			initValues.Add($"SkipWhen = () => {(details.SkipType ?? testClass).ToCSharp()}.{details.SkipWhen}");
		if (details.Timeout is not 0)
			initValues.Add($"Timeout = {details.Timeout}");

		return CodeGenTestMethodRegistration.FromTestMethodDetails(details, $"new global::Xunit.v3.FactTestCaseFactory() {{ {string.Join(", ", initValues)} }}");
	}
}
