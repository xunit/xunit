#nullable enable

#pragma warning disable IDE0290 // Use primary constructor

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators
{
	/// <summary>
	/// A source generator base class for attributes that provide traits
	/// </summary>
	/// <remarks>
	/// This class handles understanding where trait attributes are placed (assembly, collection,
	/// test class, or test method) and only requires that the developer implement <see cref="GetTraitValues"/>
	/// to determine the trait values that should be generated as a result of the attribute.
	/// </remarks>
	public abstract class TraitGenerator : XunitAttributeGenerator<TraitGeneratorResult>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TraitGenerator"/> class.
		/// </summary>
		/// <param name="fullyQualifiedAttributeTypeName">The fully qualified attribute name (e.g.,
		/// <c>Types.Xunit.AssemblyFixtureAttribute"</c> for non-generic types, or
		/// <c>Types.Xunit.AssemblyFixtureAttribute`1"</c> for generic types). This value is passed to
		/// <see cref="SyntaxValueProvider.ForAttributeWithMetadataName{T}"/>.</param>
		protected TraitGenerator(string fullyQualifiedAttributeTypeName) :
			base(fullyQualifiedAttributeTypeName)
		{ }

		/// <inheritdoc/>
		protected override void CreateSource(
			SourceProductionContext context,
			TraitGeneratorResult result)
		{
			if (result is null)
				return;

			var initialization = new StringBuilder();

			result.Traits.GenerateSource(initialization);
			if (initialization.Length == 0)
				return;

			AddInitAttribute(context, result, initialization.ToString());
		}

		/// <summary>
		/// Gets the trait attribute name/value pairs from the trait attribute.
		/// </summary>
		/// <param name="attribute">The trait attribute</param>
		protected abstract IEnumerable<(string name, string value)> GetTraitValues(AttributeData attribute);

		/// <summary>
		/// Processes the trait attribute on a test assembly.
		/// </summary>
		/// <param name="result">The generator result</param>
		/// <param name="name">The trait name</param>
		/// <param name="value">The trait value</param>
		/// <param name="testAssembly">The test assembly</param>
		/// <remarks>
		/// By default, this calls <see cref="TraitRegistration.AddAssemblyTrait"/>.
		/// </remarks>
		protected virtual void ProcessTestAssembly(
			TraitGeneratorResult result,
			string name,
			string value,
			IAssemblySymbol testAssembly) =>
				(result ?? throw new ArgumentNullException(nameof(result))).Traits.AddAssemblyTrait(name, value);

		/// <summary>
		/// Processes the trait attribute on a test collection definition.
		/// </summary>
		/// <param name="result">The generator result</param>
		/// <param name="name">The trait name</param>
		/// <param name="value">The trait value</param>
		/// <param name="testCollectionName">The test collection name (will be <see langword="null"/> for type-based collections)</param>
		/// <param name="testCollectionType">The test collection type symbol</param>
		/// <remarks>
		/// By default, this calls <see cref="TraitRegistration.AddCollectionTrait"/>.
		/// </remarks>
		protected virtual void ProcessTestCollection(
			TraitGeneratorResult result,
			string name,
			string value,
			string? testCollectionName,
			INamedTypeSymbol testCollectionType) =>
				(result ?? throw new ArgumentNullException(nameof(result))).Traits.AddCollectionTrait(testCollectionName, testCollectionType, name, value);

		/// <summary>
		/// Processes the trait attribute on a test class.
		/// </summary>
		/// <param name="result">The generator result</param>
		/// <param name="name">The trait name</param>
		/// <param name="value">The trait value</param>
		/// <param name="testClass">The test class symbol</param>
		/// <remarks>
		/// By default, this calls <see cref="TraitRegistration.AddClassTrait"/>.
		/// </remarks>
		protected virtual void ProcessTestClass(
			TraitGeneratorResult result,
			string name,
			string value,
			INamedTypeSymbol testClass) =>
				(result ?? throw new ArgumentNullException(nameof(result))).Traits.AddClassTrait(testClass, name, value);

		/// <summary>
		/// Processes the trait attribute on a test method.
		/// </summary>
		/// <param name="result">The generator result</param>
		/// <param name="name">The trait name</param>
		/// <param name="value">The trait value</param>
		/// <param name="testMethod">The test method symbol</param>
		/// <remarks>
		/// By default, this calls <see cref="TraitRegistration.AddMethodTrait"/>.
		/// </remarks>
		protected virtual void ProcessTestMethod(
			TraitGeneratorResult result,
			string name,
			string value,
			IMethodSymbol testMethod) =>
				(result ?? throw new ArgumentNullException(nameof(result))).Traits.AddMethodTrait(testMethod, name, value);

		/// <inheritdoc/>
		protected override TraitGeneratorResult? Transform(
			GeneratorAttributeSyntaxContext context,
			CancellationToken cancellationToken)
		{
			var result = new TraitGeneratorResult(context);
			Action<TraitGeneratorResult, string, string> processor;

			if (context.TargetSymbol is IAssemblySymbol assemblySymbol)
				processor = (result, name, value) => ProcessTestAssembly(result, name, value, assemblySymbol);
			else if (context.TargetSymbol is IMethodSymbol methodSymbol)
				processor = (result, name, value) => ProcessTestMethod(result, name, value, methodSymbol);
			else if (context.TargetSymbol is INamedTypeSymbol typeSymbol)
			{
				var collectionAttribute =
					typeSymbol
						.GetAttributes()
						.FirstOrDefault(a => a.AttributeClass?.ToCSharp(includeGlobal: false) == Types.Xunit.CollectionDefinitionAttribute);

				if (collectionAttribute is not null)
				{
					var testCollectionName = default(string);
					if (collectionAttribute.ConstructorArguments.Length > 0 && collectionAttribute.ConstructorArguments[0].Kind == TypedConstantKind.Primitive)
						testCollectionName = collectionAttribute.ConstructorArguments[0].Value as string;

					processor = (result, name, value) => ProcessTestCollection(result, name, value, testCollectionName, typeSymbol);
				}
				else
					processor = (result, name, value) => ProcessTestClass(result, name, value, typeSymbol);
			}
			else
				return null;

			foreach (var attribute in context.Attributes)
				foreach (var (name, value) in GetTraitValues(attribute))
					processor(result, name, value);

			return result;
		}
	}
}
