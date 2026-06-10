#nullable enable

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Generators
{
	/// <summary>
	/// An override of <see cref="TestMethodDetails"/> specifically designed to handle test methods
	/// which are decorated with <c>[Fact]</c> or <c>[CulturedFact]</c>.
	/// </summary>
	public class FactMethodDetails : TestMethodDetails
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FactMethodDetails"/> class.
		/// </summary>
		/// <param name="classSymbol">The test class symbol</param>
		/// <param name="methodDeclaration">The test method declaration</param>
		/// <param name="methodSymbol">The test method symbol</param>
		/// <param name="attribute">The <c>[Fact]</c> or <c>[CulturedFact]</c> attribute</param>
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

		/// <summary>
		/// Gets the code that invokes the test method. The assumed input of the method invoker
		/// is the test class instance, and the return value should be a <see cref="ValueTask"/>.
		/// </summary>
		/// <remarks>
		/// The generated code varies depending on:
		/// <list type="bullet">
		/// <item>If the test method is <see langword="static"/></item>
		/// <item>If the test method return <see langword="void"/> vs. <see cref="Task"/> or <see cref="ValueTask"/></item>
		/// </list>
		/// </remarks>
		public string MethodInvoker { get; }

		/// <remarks>
		/// In addition to the validation done by <see cref="TestMethodDetails.Process"/>, also ensures that
		/// the test method does not have any parameters (as that's not legal for facts).
		/// </remarks>
		/// <inheritdoc/>
		public override bool Process()
		{
			if (!base.Process())
				return false;

			return MethodSymbol.Parameters.Length == 0;
		}
	}
}
