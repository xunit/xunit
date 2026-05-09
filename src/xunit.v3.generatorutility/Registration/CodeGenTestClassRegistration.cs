#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators
{
	/// <summary>
	/// A helper class designed to perform test-class-level registration.
	/// </summary>
	public class CodeGenTestClassRegistration : IEquatable<CodeGenTestClassRegistration>
	{
		readonly string? classFactory;
		readonly Dictionary<string, string> classFixtures = new Dictionary<string, string>();
		readonly Dictionary<string, HashSet<string>> traits = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
		readonly string type;
		readonly string typeIndex;

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeGenTestClassRegistration"/> class.
		/// </summary>
		/// <param name="testClass">The test class symbol</param>
		public CodeGenTestClassRegistration(INamedTypeSymbol testClass)
		{
			if (testClass is null)
				throw new ArgumentNullException(nameof(testClass));

			classFactory = testClass.ToTestClassFactory();
			type = testClass.ToCSharp();
			typeIndex = testClass.ToTypeIndex();
		}

		/// <summary>
		/// Gets or sets the factory for the class-level test case orderer.
		/// </summary>
		public string? TestCaseOrdererFactory { get; set; }

		/// <summary>
		/// Gets the factory for the class-level test method orderer.
		/// </summary>
		public string? TestMethodOrdererFactory { get; set; }

		/// <summary>
		/// Adds a class fixture to the class fixture factories.
		/// </summary>
		/// <param name="fixtureType">The class fixture type</param>
		public void AddClassFixture(INamedTypeSymbol fixtureType)
		{
			if (fixtureType is null)
				throw new ArgumentNullException(nameof(fixtureType));

			var factory = fixtureType.ToFixtureFactory("Class");
			if (factory != null)
				classFixtures[fixtureType.ToCSharp()] = factory;
		}

		/// <summary>
		/// Adds one or more traits to the traits list.
		/// </summary>
		/// <param name="traitName">The trait name</param>
		/// <param name="traitValues">The trait values</param>
		public void AddTrait(
			string traitName,
			params string[] traitValues)
		{
			if (traitName is null)
				throw new ArgumentNullException(nameof(traitName));
			if (traitValues is null)
				throw new ArgumentNullException(nameof(traitValues));
			if (traitValues.Length == 0)
				throw new ArgumentException("Must include at least one trait value", nameof(traitValues));

			if (!traits.TryGetValue(traitName, out var hash))
			{
				hash = new HashSet<string>();
				traits[traitName] = hash;
			}

			foreach (var traitValue in traitValues)
				hash.Add(traitValue);
		}

		/// <inheritdoc/>
		public override bool Equals(object? obj) =>
			Equals(obj as CodeGenTestClassRegistration);

		/// <inheritdoc/>
		public bool Equals(CodeGenTestClassRegistration? other) =>
			other != null &&
			ComparerHelper.Equal(classFactory, other.classFactory) &&
			ComparerHelper.Equal(classFixtures, other.classFixtures) &&
			ComparerHelper.Equal(TestCaseOrdererFactory, other.TestCaseOrdererFactory) &&
			ComparerHelper.Equal(TestMethodOrdererFactory, other.TestMethodOrdererFactory) &&
			ComparerHelper.Equal(traits, other.traits) &&
			ComparerHelper.Equal(type, other.type) &&
			ComparerHelper.Equal(typeIndex, other.typeIndex);

		/// <summary>
		/// Generates the source for the test class registration.
		/// </summary>
		/// <remarks>
		/// This generates a call to <c>RegisteredEngineConfig.RegisterCodeGenTestClass</c>, followed by zero
		/// or more calls <c>RegisteredEngineConfig.RegisterCodeGenTestClassTrait</c> for the traits that
		/// were decorated on the test class.
		/// </remarks>
		public void GenerateSource(StringBuilder source)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));

			source.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterCodeGenTestClass({typeIndex.ToCSharp()}, {ToClassRegistration()});
");

			foreach (var trait in traits)
				source.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterCodeGenTestClassTrait({typeIndex.ToCSharp()}, {trait.Key.ToCSharp()}, {string.Join(", ", trait.Value.Select(v => v.ToCSharp()))});
");
		}

		/// <inheritdoc/>
		public override int GetHashCode() =>
			HashCodeHelper.Start()
				.With(classFactory)
				.With(classFixtures)
				.With(TestCaseOrdererFactory)
				.With(TestMethodOrdererFactory)
				.With(traits)
				.With(type)
				.With(typeIndex);

		string ToClassRegistration()
		{
			var classInitValues = new List<string>()
			{
				$"Class = typeof({type})",
			};

			if (classFactory != null)
				classInitValues.Add($"ClassFactory = {classFactory}");
			if (classFixtures.Count != 0)
				classInitValues.Add($"ClassFixtureFactories = {classFixtures.ToFixtureFactories()}");
			if (TestCaseOrdererFactory != null)
				classInitValues.Add($"TestCaseOrdererFactory = () => {TestCaseOrdererFactory}");
			if (TestMethodOrdererFactory != null)
				classInitValues.Add($"TestMethodOrdererFactory = () => {TestMethodOrdererFactory}");

			return $"new global::Xunit.v3.CodeGenTestClassRegistration() {{ {string.Join(", ", classInitValues)} }}";
		}
	}
}
