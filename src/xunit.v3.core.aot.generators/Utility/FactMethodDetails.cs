using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Generators;

public class FactMethodDetails : MethodDetailsBase
{
	public FactMethodDetails(
		INamedTypeSymbol classSymbol,
		MethodDeclarationSyntax methodDeclaration,
		IMethodSymbol methodSymbol,
		AttributeData attribute) :
			base(classSymbol, methodDeclaration, methodSymbol, attribute)
	{
		MethodInvoker = (classSymbol.IsStatic || MethodSymbol.IsStatic, MethodSymbol.ReturnType.SpecialType == SpecialType.System_Void) switch
		{
			// Static, returning void
			(true, true) => $"async _ => {classSymbol.ToCSharp()}.{MethodSymbol.Name}()",
			// Static, returning non-void
			(true, false) => $"_ => global::Xunit.Sdk.AsyncUtility.Await({classSymbol.ToCSharp()}.{MethodSymbol.Name}())",
			// Non-static, returning void
			(false, true) => $"async obj => (({classSymbol.ToCSharp()})obj!).{MethodSymbol.Name}()",
			// Non-static, returning non-void
			(false, false) => $"obj => global::Xunit.Sdk.AsyncUtility.Await((({classSymbol.ToCSharp()})obj!).{MethodSymbol.Name}())",
		};
	}

	public string MethodInvoker { get; }

	public override void Process()
	{
		base.Process();

		if (MethodDeclaration.ParameterList.Parameters.Count != 0)
			Diagnostics.Add(
				Diagnostic.Create(
					DiagnosticDescriptors.X1001_FactMethodMustNotHaveParameters,
					MethodDeclaration.ParameterList.GetLocation()
				)
			);

		if (MethodDeclaration.TypeParameterList is not null && MethodDeclaration.TypeParameterList.Parameters.Count != 0)
			Diagnostics.Add(
				Diagnostic.Create(
					DiagnosticDescriptors.X9009_FactCannotBeGeneric,
					MethodDeclaration.TypeParameterList.GetLocation()
				)
			);
	}
}
