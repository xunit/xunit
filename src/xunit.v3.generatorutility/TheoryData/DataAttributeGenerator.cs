#nullable enable

using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators
{
	/// <summary>
	/// A base type for source generators for attributes derived from <c>Xunit.v3.DataAttribute</c>.
	/// </summary>
	public abstract class DataAttributeGenerator : XunitAttributeGenerator<DataAttributeGeneratorResult>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DataAttributeGenerator"/> class.
		/// </summary>
		/// <param name="fullyQualifiedAttributeType">The fully qualified attribute type name</param>
		protected DataAttributeGenerator(string fullyQualifiedAttributeType) :
			base(fullyQualifiedAttributeType)
		{ }

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
			DataAttributeGeneratorResult result)
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
			DataAttributeGeneratorResult result,
			CancellationToken cancellationToken);

		/// <inheritdoc/>
		protected override DataAttributeGeneratorResult? Transform(
			GeneratorAttributeSyntaxContext context,
			CancellationToken cancellationToken)
		{
			var testMethod = context.TargetSymbol as IMethodSymbol;
			if (testMethod is null)
				return null;

			var testClass = testMethod.ContainingType;
			if (testClass is null)
				return null;

			var result = new DataAttributeGeneratorResult(context, testClass, testMethod)
			{
				GeneratorSuffix = $"{testClass.Name}٠{testMethod.Name}٠",
			};

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
