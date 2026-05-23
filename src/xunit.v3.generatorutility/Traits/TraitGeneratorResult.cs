#nullable enable

#pragma warning disable IDE0290 // Use primary constructor

using System;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators
{
	/// <summary>
	/// Result class for <see cref="TraitGenerator"/>
	/// </summary>
	public class TraitGeneratorResult : XunitGeneratorResult, IEquatable<TraitGeneratorResult>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TraitGeneratorResult"/> class.
		/// </summary>
		/// <param name="context">The generator attribute syntax context</param>
		public TraitGeneratorResult(GeneratorAttributeSyntaxContext context) :
			base(context.SemanticModel.SyntaxTree.FilePath, context.TargetNode.GetLocation())
		{ }

		/// <summary>
		/// Gets the traits registration, used to add traits to the generated source.
		/// </summary>
		public TraitRegistration Traits { get; } = new();

		/// <inheritdoc/>
		public override bool Equals(object? obj) =>
			Equals(obj as TraitRegistration);

		/// <inheritdoc/>
		public bool Equals(TraitGeneratorResult? other) =>
			other != null &&
			base.Equals(other) &&
			ComparerHelper.Equal(Traits, other.Traits);

		/// <inheritdoc/>
		public override int GetHashCode() =>
			HashCodeHelper.Extend(base.GetHashCode()).With(Traits);
	}
}
