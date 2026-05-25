#nullable enable

#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0090 // Use 'new(...)'

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators
{
	/// <summary>
	/// A helper class designed to perform test-collection-level registration.
	/// </summary>
	public sealed class CodeGenTestCollectionRegistration : IEquatable<CodeGenTestCollectionRegistration?>
	{
		readonly Dictionary<string, string> classFixtures = new Dictionary<string, string>();
		readonly Dictionary<string, string> collectionFixtures = new Dictionary<string, string>();
		readonly string? collectionName;
		readonly string? collectionType;

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeGenTestCollectionRegistration"/> class.
		/// </summary>
		/// <param name="collectionName">The optional collection name</param>
		/// <param name="collectionType">The optional collection type</param>
		/// <remarks>
		/// It is required that one of <paramref name="collectionName"/> or <paramref name="collectionType"/>
		/// will be non-<see langword="null"/>.
		/// </remarks>
		public CodeGenTestCollectionRegistration(
			string? collectionName,
			INamedTypeSymbol? collectionType)
		{
			if (collectionName is null && collectionType is null)
				throw new ArgumentNullException($"One of {nameof(collectionName)} or {nameof(collectionType)} must be non-null", default(Exception));

			this.collectionName = collectionName;
			this.collectionType = collectionType?.ToCSharp();
		}

		/// <summary>
		/// A flag indicating whether this collection wants to run without being parallelized against
		/// other test collections.
		/// </summary>
		public bool DisableParallelization { get; set; }

		/// <summary>
		/// Options which determine the amount of parallelization to allow for this test collection.
		/// </summary>
		public int? ParallelismOptions { get; set; }

		/// <summary>
		/// Gets the factory for the collection-level test case orderer.
		/// </summary>
		public string? TestCaseOrdererFactory { get; set; }

		/// <summary>
		/// Gets the factory for the collection-level test class orderer.
		/// </summary>
		public string? TestClassOrdererFactory { get; set; }

		/// <summary>
		/// Gets the factory for the collection-level test method orderer.
		/// </summary>
		public string? TestMethodOrdererFactory { get; set; }

		/// <summary>
		/// Adds a class fixture to the class fixture factories.
		/// </summary>
		/// <param name="fixtureType">The class fixture type</param>
		/// <remarks>
		/// The fixture type is validated, and will only be added to the factory list if it passes
		/// all validation:
		/// <list type="bullet">
		/// <item>Type must be public or internal</item>
		/// </list>
		/// </remarks>
		public void AddClassFixture(INamedTypeSymbol fixtureType)
		{
			if (fixtureType is null)
				throw new ArgumentNullException(nameof(fixtureType));

			var factory = fixtureType.ToFixtureFactory("Class");
			if (factory != null)
				classFixtures[fixtureType.ToCSharp()] = factory;
		}

		/// <summary>
		/// Adds a collection fixture to the collection fixture factories.
		/// </summary>
		/// <param name="fixtureType">The collection fixture type</param>
		/// <remarks>
		/// The fixture type is validated, and will only be added to the factory list if it passes
		/// all validation:
		/// <list type="bullet">
		/// <item>Type must be public or internal</item>
		/// </list>
		/// </remarks>
		public void AddCollectionFixture(INamedTypeSymbol fixtureType)
		{
			if (fixtureType is null)
				throw new ArgumentNullException(nameof(fixtureType));

			var factory = fixtureType.ToFixtureFactory("Collection");
			if (factory != null)
				collectionFixtures[fixtureType.ToCSharp()] = factory;
		}

		/// <inheritdoc/>
		public override bool Equals(object? obj) =>
			Equals(obj as CodeGenTestCollectionRegistration);

		/// <inheritdoc/>
		public bool Equals(CodeGenTestCollectionRegistration? other) =>
			other != null &&
			ComparerHelper.Equal(classFixtures, other.classFixtures) &&
			ComparerHelper.Equal(collectionFixtures, other.collectionFixtures) &&
			ComparerHelper.Equal(collectionType, other.collectionType) &&
			ComparerHelper.Equal(DisableParallelization, other.DisableParallelization) &&
			ComparerHelper.Equal(ParallelismOptions, other.ParallelismOptions) &&
			ComparerHelper.Equal(TestCaseOrdererFactory, other.TestCaseOrdererFactory) &&
			ComparerHelper.Equal(TestClassOrdererFactory, other.TestClassOrdererFactory) &&
			ComparerHelper.Equal(TestMethodOrdererFactory, other.TestMethodOrdererFactory);

		/// <summary>
		/// Generates the source for the test collection.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to generate the source into</param>
		/// <remarks>
		/// This generates a call to <c>RegisteredEngineConfig.RegisterCollectionDefinition</c> and zero or
		/// more calls to <c>RegisteredEngineConfig.RegisterCodeGenTestCollectionTrait</c> for the traits
		/// attached to the test collection.
		/// </remarks>
		public void GenerateSource(StringBuilder builder)
		{
			if (builder is null)
				throw new ArgumentNullException(nameof(builder));

			var name = collectionName.ToCSharp();

			builder.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterCollectionDefinition({name}, {ToCollectionRegistration()});
");
		}

		/// <inheritdoc/>
		public override int GetHashCode() =>
			HashCodeHelper.Start()
				.With(classFixtures)
				.With(collectionFixtures)
				.With(collectionType)
				.With(DisableParallelization)
				.With(ParallelismOptions)
				.With(TestCaseOrdererFactory)
				.With(TestClassOrdererFactory)
				.With(TestMethodOrdererFactory);

		string ToCollectionRegistration()
		{
			var initValues = new List<string>();

			if (classFixtures.Count != 0)
				initValues.Add($"ClassFixtureFactories = {classFixtures.ToFixtureFactories()}");
			if (collectionFixtures.Count != 0)
				initValues.Add($"CollectionFixtureFactories = {collectionFixtures.ToFixtureFactories()}");
			if (ParallelismOptions.HasValue)
				initValues.Add($"ParallelismOptions = (global::Xunit.Sdk.ParallelismOptions){ParallelismOptions}");
			if (DisableParallelization)
				initValues.Add("ParallelismOptions = global::Xunit.Sdk.ParallelismOptions.None");
			if (TestCaseOrdererFactory != null)
				initValues.Add($"TestCaseOrdererFactory = () => {TestCaseOrdererFactory}");
			if (TestClassOrdererFactory != null)
				initValues.Add($"TestClassOrdererFactory = () => {TestClassOrdererFactory}");
			if (TestMethodOrdererFactory != null)
				initValues.Add($"TestMethodOrdererFactory = () => {TestMethodOrdererFactory}");
			if (collectionType != null)
				initValues.Add($"Type = typeof({collectionType})");

			if (initValues.Count == 0)
				return "global::Xunit.v3.CodeGenTestCollectionRegistration.Empty";

			return $"new global::Xunit.v3.CodeGenTestCollectionRegistration() {{ {string.Join(", ", initValues)} }}";
		}
	}
}
