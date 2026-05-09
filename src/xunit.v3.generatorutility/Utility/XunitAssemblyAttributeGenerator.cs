#nullable enable

using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators
{
	/// <summary>
	/// A base class for generating xUnit.net test assembly registration from assembly-level attributes.
	/// </summary>
	/// <remarks>
	/// This base class uses an instance of <see cref="CodeGenTestAssemblyRegistration"/> to contain the
	/// registration items to generate source for. Implementers will use this to record the work to be
	/// registered.
	/// </remarks>
	public abstract class XunitAssemblyAttributeGenerator : XunitAttributeGenerator<XunitAssemblyAttributeGeneratorResult>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="XunitAssemblyAttributeGenerator"/> class.
		/// </summary>
		/// <param name="fullyQualifiedAttributeTypeName">The fully qualified attribute type name</param>
		protected XunitAssemblyAttributeGenerator(string fullyQualifiedAttributeTypeName) :
			base(fullyQualifiedAttributeTypeName)
		{ }

		/// <inheritdoc/>
		protected override void CreateSource(
			SourceProductionContext context,
			XunitAssemblyAttributeGeneratorResult result)
		{
			if (result is null || !result.ShouldGenerate)
				return;

			var builder = new StringBuilder();
			result.Registration.GenerateSource(builder);

			if (builder.Length != 0)
				AddInitAttribute(context, result, builder.ToString());
		}

		/// <summary>
		/// Override this to process the attribute.
		/// </summary>
		/// <param name="semanticModel">The semantic model</param>
		/// <param name="registration">The registration that will be used to generate the source</param>
		/// <param name="attribute">The attribute</param>
		protected abstract void ProcessAttribute(
			SemanticModel semanticModel,
			CodeGenTestAssemblyRegistration registration,
			AttributeData attribute);

		/// <inheritdoc/>
		protected override XunitAssemblyAttributeGeneratorResult? Transform(
			GeneratorAttributeSyntaxContext context,
			CancellationToken cancellationToken)
		{
			if (context.TargetSymbol is IAssemblySymbol)
			{
				var result = new XunitAssemblyAttributeGeneratorResult(context.SemanticModel.SyntaxTree.FilePath, context.TargetNode.GetLocation());

				foreach (var attribute in context.Attributes)
					ProcessAttribute(context.SemanticModel, result.Registration, attribute);

				return result;
			}

			return null;
		}
	}
}
