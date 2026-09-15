#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Generators
{
	/// <summary>
	/// Interface to be implemented to support custom test method generators.
	/// </summary>
	/// <remarks>
	/// The implementation class must be decorated with <see cref="TestMethodGeneratorAttribute"/> to
	/// indicate the supported attribute(s) which this generator can support.
	/// </remarks>
	public interface ITestMethodGenerator
	{
		/// <summary>
		/// Override to return the test method registration.
		/// </summary>
		/// <param name="semanticModel">The semantic model</param>
		/// <param name="testClass">The test class symbol</param>
		/// <param name="testMethodSyntax">The declaration syntax of the test method</param>
		/// <param name="testMethod">The test method symbol</param>
		/// <param name="attribute">The attribute instance</param>
		/// <returns>Returns the registration, or <see langword="null"/> if the test method is invalid</returns>
		CodeGenTestMethodRegistration? GetTestMethodRegistration(
			SemanticModel semanticModel,
			INamedTypeSymbol testClass,
			MethodDeclarationSyntax testMethodSyntax,
			IMethodSymbol testMethod,
			AttributeData attribute);
	}
}
