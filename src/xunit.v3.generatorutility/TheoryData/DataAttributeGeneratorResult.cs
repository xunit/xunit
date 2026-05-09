#nullable enable

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators
{
	/// <summary>
	/// The generator result for <see cref="DataAttributeGenerator"/>
	/// </summary>
	public class DataAttributeGeneratorResult : XunitGeneratorResult, IEquatable<DataAttributeGeneratorResult?>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DataAttributeGeneratorResult"/> class.
		/// </summary>
		/// <param name="context">The attribute syntax context</param>
		/// <param name="testClass">The test class symbol</param>
		/// <param name="testMethod">The test method symbol</param>
		public DataAttributeGeneratorResult(
			GeneratorAttributeSyntaxContext context,
			INamedTypeSymbol testClass,
			IMethodSymbol testMethod) :
				base(context.SemanticModel.SyntaxTree.FilePath, context.TargetNode.GetLocation())
		{
			MethodName = testMethod?.Name ?? throw new ArgumentNullException(nameof(testMethod));
			Type = testClass?.ToTypeIndex() ?? throw new ArgumentNullException(nameof(testClass));
		}

		/// <summary>
		/// Gets the theory data row factories.
		/// </summary>
		public List<TheoryDataRowFactory> Factories { get; } = new List<TheoryDataRowFactory>();

		/// <summary>
		/// Gets the test method name.
		/// </summary>
		public string MethodName { get; }

		/// <summary>
		/// Gets the test class type name.
		/// </summary>
		public string Type { get; }

		/// <inheritdoc/>
		public override bool Equals(object? obj) =>
			Equals(obj as DataAttributeGeneratorResult);

		/// <inheritdoc/>
		public bool Equals(DataAttributeGeneratorResult? other) =>
			other != null &&
			base.Equals(other) &&
			ComparerHelper.Equal(Factories, other.Factories) &&
			ComparerHelper.Equal(MethodName, other.MethodName) &&
			ComparerHelper.Equal(Type, other.Type);

		/// <inheritdoc/>
		public override int GetHashCode() =>
			HashCodeHelper.Extend(base.GetHashCode()).With(Factories).With(MethodName).With(Type);
	}
}
