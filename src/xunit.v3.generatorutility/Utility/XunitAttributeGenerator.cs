#nullable enable

#pragma warning disable IDE0290 // Use primary constructor

using System;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators
{
	/// <summary>
	/// A base class for generating xUnit.net registration source from attributes.
	/// </summary>
	/// <typeparam name="TResult">The result type (must derive from <see cref="XunitGeneratorResult"/>)</typeparam>
	public abstract class XunitAttributeGenerator<TResult> : XunitGenerator
		where TResult : XunitGeneratorResult
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="XunitAttributeGenerator{TResult}"/> class.
		/// </summary>
		/// <param name="fullyQualifiedAttributeTypeName">The fully qualified attribute name (e.g.,
		/// <c>Types.Xunit.AssemblyFixtureAttribute"</c> for non-generic types, or
		/// <c>Types.Xunit.AssemblyFixtureAttribute`1"</c> for generic types). This value is passed to
		/// <see cref="SyntaxValueProvider.ForAttributeWithMetadataName{T}"/>.</param>
		protected XunitAttributeGenerator(string fullyQualifiedAttributeTypeName) =>
			FullyQualifiedAttributeTypeName = fullyQualifiedAttributeTypeName ?? throw new ArgumentNullException(nameof(fullyQualifiedAttributeTypeName));

		/// <summary>
		/// Gets the fully qualified attribute type name (from the constructor)
		/// </summary>
		protected string FullyQualifiedAttributeTypeName { get; }

		/// <summary>
		/// Override to create the source code from the transformation result.
		/// </summary>
		/// <param name="context">The source production context</param>
		/// <param name="result">The transformation result</param>
		protected abstract void CreateSource(
			SourceProductionContext context,
			TResult result);

		void CreateSourceInternal(
			SourceProductionContext context,
			TResult result)
		{
			if (result.ShouldGenerate)
				CreateSource(context, result);
		}

		/// <inheritdoc/>
		protected override sealed void Initialize(
			IncrementalGeneratorInitializationContext context,
			IncrementalValueProvider<XunitMSBuildProperties> properties)
		{
			var result =
				context
					.SyntaxProvider
					.ForAttributeWithMetadataName(FullyQualifiedAttributeTypeName, ValidateAttribute, Transform)
					.WhereNotNull()
					.Combine(properties)
					.Select((pair, _) =>
					{
						pair.Left.Initialize(pair.Right);
						return pair.Left;
					});

			context.RegisterSourceOutput(result, CreateSourceInternal);
		}

		/// <summary>
		/// Transforms the attribute syntax into a result object.
		/// </summary>
		/// <param name="context">The attribute syntax context</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns>A result object, if there should be source generated; <see langword="null"/>, otherwise</returns>
		protected abstract TResult? Transform(
			GeneratorAttributeSyntaxContext context,
			CancellationToken cancellationToken);

		/// <summary>
		/// Override to validate the attribute is processable.
		/// </summary>
		/// <param name="syntaxNode">The attribute syntax node</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		protected virtual bool ValidateAttribute(
			SyntaxNode syntaxNode,
			CancellationToken cancellationToken) =>
				true;
	}
}
