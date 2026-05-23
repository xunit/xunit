#nullable enable

#pragma warning disable IDE0290 // Use primary constructor

using System;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators
{
	/// <summary>
	/// The generator result for <see cref="XunitAssemblyAttributeGenerator"/>.
	/// </summary>
	public class XunitAssemblyAttributeGeneratorResult : XunitGeneratorResult, IEquatable<XunitAssemblyAttributeGeneratorResult>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="XunitAssemblyAttributeGeneratorResult"/> class.
		/// </summary>
		/// <param name="sourceFilePath">The source file path of the syntax tree (typically retrieved by
		/// calling <c>context.SemanticModel.SyntaxTree.FilePath</c>)</param>
		/// <param name="syntaxLocation">The location of the syntax node (typically retrieved by
		/// calling <c>context.TargetNode.GetLocation()</c>)</param>
		/// <remarks>The <paramref name="sourceFilePath"/> and <paramref name="syntaxLocation"/> are used to
		/// help construct a unique name for the init attribute.</remarks>
		public XunitAssemblyAttributeGeneratorResult(
			string sourceFilePath,
			Location syntaxLocation) :
				base(sourceFilePath, syntaxLocation)
		{ }

		/// <summary>
		/// Gets the registration that will be used to generate the source
		/// </summary>
		public CodeGenTestAssemblyRegistration Registration { get; } = new CodeGenTestAssemblyRegistration();

		/// <inheritdoc/>
		public override bool Equals(object? obj) =>
			Equals(obj as XunitAssemblyAttributeGeneratorResult);

		/// <inheritdoc/>
		public bool Equals(XunitAssemblyAttributeGeneratorResult? other) =>
			other != null &&
			ComparerHelper.Equal(Registration, other.Registration);

		/// <inheritdoc/>
		public override int GetHashCode() =>
			HashCodeHelper.Start()
				.With(Registration);
	}
}
