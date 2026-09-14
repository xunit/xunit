#nullable enable

#pragma warning disable IDE0290 // Use primary constructor

using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
		/// Override to create the instance of <typeparamref name="TResult"/> for a test method which is inherited
		/// from a base class declared in a referenced assembly (and therefore has no attribute syntax).
		/// </summary>
		/// <param name="testClassSymbol">The class which declares the test method</param>
		/// <param name="testMethodSymbol">The test method</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns>The result, or <see langword="null"/> to skip generating theory data for the test method
		/// (the default behavior)</returns>
		protected virtual TResult? CreateResultForInheritedMethod(
			INamedTypeSymbol testClassSymbol,
			IMethodSymbol testMethodSymbol,
			CancellationToken cancellationToken) =>
				null;

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
		/// <remarks>
		/// Attributes on test methods declared in referenced assemblies aren't visible via the attribute syntax, so
		/// this finds them by walking the base classes of every class declared in source. Theory data is registered
		/// against the declaring type, so each inherited test method is only generated once, regardless of how many
		/// classes derive from its declaring type.
		/// </remarks>
		protected override IncrementalValuesProvider<TResult>? GetAdditionalResults(IncrementalGeneratorInitializationContext context) =>
			context
				.SyntaxProvider
				.CreateSyntaxProvider(
					(syntaxNode, _) => syntaxNode is ClassDeclarationSyntax { BaseList: not null },
					TransformInheritedMethods
				)
				.SelectMany((results, _) => results)
				.Collect()
				.SelectMany((results, _) => results.GroupBy(result => result.InitAttributeNameSuffix).Select(group => group.First()));

		static string GetFullyQualifiedMetadataName(INamedTypeSymbol type)
		{
			var result = type.MetadataName;

			for (var containingType = type.ContainingType; containingType is not null; containingType = containingType.ContainingType)
				result = containingType.MetadataName + "+" + result;

			if (type.ContainingNamespace is { IsGlobalNamespace: false } containingNamespace)
				result = containingNamespace.ToDisplayString() + "." + result;

			return result;
		}

		ImmutableArray<TResult> TransformInheritedMethods(
			GeneratorSyntaxContext context,
			CancellationToken cancellationToken)
		{
			if (context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken) is not INamedTypeSymbol classSymbol)
				return ImmutableArray<TResult>.Empty;

			var results = ImmutableArray.CreateBuilder<TResult>();

			for (var baseClassSymbol = classSymbol.BaseType; baseClassSymbol is not null && baseClassSymbol.SpecialType != SpecialType.System_Object; baseClassSymbol = baseClassSymbol.BaseType)
			{
				// Base classes declared in source are handled by the attribute syntax
				if (baseClassSymbol.DeclaringSyntaxReferences.Length != 0)
					continue;

				var testClass = baseClassSymbol.OriginalDefinition;

				foreach (var testMethod in testClass.GetMembers().OfType<IMethodSymbol>())
				{
					if (testMethod.MethodKind != MethodKind.Ordinary)
						continue;

					var attributes =
						testMethod
							.GetAttributes()
							.Where(attribute => attribute.AttributeClass is not null && GetFullyQualifiedMetadataName(attribute.AttributeClass.OriginalDefinition) == FullyQualifiedAttributeTypeName)
							.ToArray();

					if (attributes.Length == 0)
						continue;

					var result = CreateResultForInheritedMethod(testClass, testMethod, cancellationToken);
					if (result is null)
						continue;

					foreach (var attribute in attributes)
						ProcessAttribute(
							context.SemanticModel,
							testClass,
							testMethod,
							attribute,
							result,
							cancellationToken
						);

					if (result.Factories.Count != 0)
						results.Add(result);
				}
			}

			return results.ToImmutable();
		}

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

	/// <inheritdoc/>
	protected override DataAttributeGeneratorResult? CreateResultForInheritedMethod(
		INamedTypeSymbol testClassSymbol,
		IMethodSymbol testMethodSymbol,
		CancellationToken cancellationToken) =>
			new DataAttributeGeneratorResult(testClassSymbol, testMethodSymbol);
}
