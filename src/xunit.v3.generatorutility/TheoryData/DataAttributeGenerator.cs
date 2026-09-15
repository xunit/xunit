#nullable enable

#pragma warning disable IDE0290 // Use primary constructor

using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Xunit.Generators;

namespace Xunit.Generators
{
	/// <summary>
	/// A base type for source generators for attributes derived from <c>Xunit.v3.DataAttribute</c>
	/// with a configurable generator result type
	/// </summary>
	public abstract class DataAttributeGenerator<TResult> : XunitAttributeGenerator<TResult>
		where TResult : DataAttributeGeneratorResult
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DataAttributeGenerator{TResult}"/> class.
		/// </summary>
		/// <param name="fullyQualifiedAttributeType">The fully qualified attribute type name</param>
		protected DataAttributeGenerator(string fullyQualifiedAttributeType) :
			base(fullyQualifiedAttributeType)
		{ }

		/// <summary>
		/// Override to create the instance of <typeparamref name="TResult"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="testClassSymbol"></param>
		/// <param name="testMethodSymbol"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		protected abstract TResult CreateResult(
			GeneratorAttributeSyntaxContext context,
			INamedTypeSymbol testClassSymbol,
			IMethodSymbol testMethodSymbol,
			CancellationToken cancellationToken);

		/// <summary>
		/// Generates the source for the theory data row factories.
		/// </summary>
		/// <param name="context">The generation context</param>
		/// <param name="result">The result from the transformation</param>
		/// <remarks>
		/// This method generates a new init attribute, with one or more calls to
		/// <c>RegisteredEngineConfig.RegisterTheoryDataRowFactory</c> for each of the theory
		/// data row factories.
		/// </remarks>
		protected override sealed void CreateSource(
			SourceProductionContext context,
			TResult result)
		{
			if (result is null || result.Factories.Count == 0)
				return;

			var initialization = new StringBuilder();

			foreach (var factory in result.Factories)
				initialization.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterTheoryDataRowFactory({result.Type.ToCSharp()}, {result.MethodName.ToCSharp()}, {factory.DisableDiscoveryEnumeration.ToCSharp()},
	{factory.Factory.Replace("\n", "\n\t")}
);
");

			AddInitAttribute(context, result, initialization.ToString());
		}

		/// <summary>
		/// Override to process the data attribute.
		/// </summary>
		/// <param name="semanticModel">The semantic model</param>
		/// <param name="testClass">The test class symbol</param>
		/// <param name="testMethod">The test method symbol</param>
		/// <param name="attribute">The data attribute</param>
		/// <param name="result">The transformation result</param>
		/// <param name="cancellationToken">The cancellation token</param>
		protected abstract void ProcessAttribute(
			SemanticModel semanticModel,
			INamedTypeSymbol testClass,
			IMethodSymbol testMethod,
			AttributeData attribute,
			TResult result,
			CancellationToken cancellationToken);

		/// <inheritdoc/>
		protected override TResult? Transform(
			GeneratorAttributeSyntaxContext context,
			CancellationToken cancellationToken)
		{
			if (context.TargetSymbol is not IMethodSymbol testMethod)
				return null;

			var testClass = testMethod.ContainingType;
			if (testClass is null)
				return null;

			var result = CreateResult(context, testClass, testMethod, cancellationToken);

			foreach (var attribute in context.Attributes)
				ProcessAttribute(
					context.SemanticModel,
					testClass,
					testMethod,
					attribute,
					result,
					cancellationToken
				);

			return result.Factories.Count == 0 ? null : result;
		}
	}
}

/// <summary>
/// A base type for source generators for attributes derived from <c>Xunit.v3.DataAttribute</c>
/// which use <see cref="DataAttributeGeneratorResult"/> as the result type
/// </summary>
public abstract class DataAttributeGenerator : DataAttributeGenerator<DataAttributeGeneratorResult>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DataAttributeGenerator{TResult}"/> class.
	/// </summary>
	/// <param name="fullyQualifiedAttributeType">The fully qualified attribute type name</param>
	protected DataAttributeGenerator(string fullyQualifiedAttributeType) :
		base(fullyQualifiedAttributeType)
	{ }

	/// <inheritdoc/>
	protected override DataAttributeGeneratorResult CreateResult(
		GeneratorAttributeSyntaxContext context,
		INamedTypeSymbol testClassSymbol,
		IMethodSymbol testMethodSymbol,
		CancellationToken cancellationToken) =>
			new DataAttributeGeneratorResult(context, testClassSymbol, testMethodSymbol);
}
