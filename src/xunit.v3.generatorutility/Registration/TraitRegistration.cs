#nullable enable

#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0090 // Use 'new(...)'

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators
{
	/// <summary>
	/// A helper class designed to do trait registration.
	/// </summary>
	public class TraitRegistration : IEquatable<TraitRegistration>
	{
		readonly Dictionary<string, HashSet<string>> assemblyTraits = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
		readonly Dictionary<string, Dictionary<string, HashSet<string>>> classTraits = new Dictionary<string, Dictionary<string, HashSet<string>>>(StringComparer.OrdinalIgnoreCase);
		readonly Dictionary<(string? TestCollectionName, string TestCollectionType), Dictionary<string, HashSet<string>>> collectionTraits = new Dictionary<(string? TestCollectionName, string TestCollectionType), Dictionary<string, HashSet<string>>>();
		readonly Dictionary<(string TestClassIndex, string MethodName), Dictionary<string, HashSet<string>>> methodTraits = new Dictionary<(string TestClassIndex, string MethodName), Dictionary<string, HashSet<string>>>();

		static void Add(
			Dictionary<string, HashSet<string>> traits,
			string name,
			string value)
		{
			if (!traits.TryGetValue(name, out var hash))
			{
				hash = new HashSet<string>();
				traits.Add(name, hash);
			}

			hash.Add(value);
		}

		/// <summary>
		/// Adds an assembly-level trait
		/// </summary>
		/// <param name="name">The trait name</param>
		/// <param name="values">The trait values</param>
		public void AddAssemblyTrait(
			string name,
			params string[] values)
		{
			if (name is null)
				throw new ArgumentNullException(nameof(name));
			if (values is null)
				throw new ArgumentNullException(nameof(values));

			foreach (var value in values)
				Add(assemblyTraits, name, value);
		}

		/// <summary>
		/// Adds a class-level trait
		/// </summary>
		/// <param name="testClass">The test class symbol</param>
		/// <param name="name">The trait name</param>
		/// <param name="values">The trait values</param>
		public void AddClassTrait(
			INamedTypeSymbol testClass,
			string name,
			params string[] values)
		{
			if (testClass is null)
				throw new ArgumentNullException(nameof(testClass));
			if (name is null)
				throw new ArgumentNullException(nameof(name));
			if (values is null)
				throw new ArgumentNullException(nameof(values));

			var key = testClass.ToCSharp();

			if (!classTraits.TryGetValue(key, out var dictionary))
			{
				dictionary = new(StringComparer.OrdinalIgnoreCase);
				classTraits.Add(key, dictionary);
			}

			foreach (var value in values)
				Add(dictionary, name, value);
		}

		/// <summary>
		/// Adds a collection-level trait
		/// </summary>
		/// <param name="testCollectionName">The optional test collection name</param>
		/// <param name="testCollectionType">The test collection type</param>
		/// <param name="name">The trait name</param>
		/// <param name="values">The trait values</param>
		public void AddCollectionTrait(
			string? testCollectionName,
			ITypeSymbol testCollectionType,
			string name,
			params string[] values)
		{
			if (testCollectionType is null)
				throw new ArgumentNullException(nameof(testCollectionType));
			if (name is null)
				throw new ArgumentNullException(nameof(name));
			if (values is null)
				throw new ArgumentNullException(nameof(values));

			var key = (testCollectionName, testCollectionType.ToCSharp());

			if (!collectionTraits.TryGetValue(key, out var dictionary))
			{
				dictionary = new(StringComparer.OrdinalIgnoreCase);
				collectionTraits.Add(key, dictionary);
			}

			foreach (var value in values)
				Add(dictionary, name, value);
		}

		/// <summary>
		/// Adds a method-level trait
		/// </summary>
		/// <param name="testMethod">The test method</param>
		/// <param name="name">The trait name</param>
		/// <param name="values">The trait values</param>
		public void AddMethodTrait(
			IMethodSymbol testMethod,
			string name,
			params string[] values)
		{
			if (testMethod is null)
				throw new ArgumentNullException(nameof(testMethod));
			if (name is null)
				throw new ArgumentNullException(nameof(name));
			if (values is null)
				throw new ArgumentNullException(nameof(values));

			var key = (testMethod.ContainingType.ToCSharp(), testMethod.Name);

			if (!methodTraits.TryGetValue(key, out var dictionary))
			{
				dictionary = new(StringComparer.OrdinalIgnoreCase);
				methodTraits.Add(key, dictionary);
			}

			foreach (var value in values)
				Add(dictionary, name, value);
		}

		/// <inheritdoc/>
		public override bool Equals(object? obj) =>
			Equals(obj as TraitRegistration);

		/// <inheritdoc/>
		public bool Equals(TraitRegistration? other) =>
			other != null &&
			ComparerHelper.Equal(assemblyTraits, other.assemblyTraits) &&
			ComparerHelper.Equal(classTraits, other.classTraits) &&
			ComparerHelper.Equal(collectionTraits, other.collectionTraits) &&
			ComparerHelper.Equal(methodTraits, other.methodTraits);

		/// <summary>
		/// Generates the trait registration source.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to generate the source into</param>
		public void GenerateSource(StringBuilder builder)
		{
			if (builder is null)
				throw new ArgumentNullException(nameof(builder));

			foreach (var trait in assemblyTraits)
				builder.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterAssemblyTrait({trait.Key.ToCSharp()}, {string.Join(", ", trait.Value.Select(v => v.ToCSharp()))});
");

			foreach (var kvp in classTraits)
				foreach (var trait in kvp.Value)
					builder.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterCodeGenTestClassTrait({kvp.Key.ToCSharp()}, {trait.Key.ToCSharp()}, {string.Join(", ", trait.Value.Select(v => v.ToCSharp()))});
");

			foreach (var kvp in collectionTraits)
				foreach (var trait in kvp.Value)
					builder.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterCodeGenTestCollectionTrait({kvp.Key.TestCollectionName.ToCSharp()}, typeof({kvp.Key.TestCollectionType}), {trait.Key.ToCSharp()}, {string.Join(", ", trait.Value.Select(v => v.ToCSharp()))});
");

			foreach (var kvp in methodTraits)
				foreach (var trait in kvp.Value)
					builder.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterCodeGenTestMethodTrait({kvp.Key.TestClassIndex.ToCSharp()}, {kvp.Key.MethodName.ToCSharp()}, {trait.Key.ToCSharp()}, {string.Join(", ", trait.Value.Select(v => v.ToCSharp()))});
");
		}

		/// <inheritdoc/>
		public override int GetHashCode() =>
			HashCodeHelper.Start().With(assemblyTraits).With(classTraits).With(collectionTraits).With(methodTraits);
	}
}
