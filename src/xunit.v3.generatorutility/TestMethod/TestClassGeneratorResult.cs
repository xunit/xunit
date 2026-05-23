#nullable enable

#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0290 // Use primary constructor

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators
{
	/// <summary>
	/// Result class for <see cref="TestClassGenerator"/>.
	/// </summary>
	public class TestClassGeneratorResult :
		XunitGeneratorResult, IEquatable<TestClassGeneratorResult?>
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="TestClassGenerator"/> class.
		/// </summary>
		/// <param name="context">The generator context</param>
		public TestClassGeneratorResult(GeneratorSyntaxContext context) :
			base(context.SemanticModel.SyntaxTree.FilePath, context.Node.GetLocation())
		{ }

		/// <summary>
		/// Gets or sets the test class registration
		/// </summary>
		public CodeGenTestClassRegistration? TestClass { get; set; }

		/// <summary>
		/// Gets a list of test methods in the test class
		/// </summary>
		public List<CodeGenTestMethodRegistration> TestMethods = new List<CodeGenTestMethodRegistration>();

		/// <inheritdoc/>
		public override bool Equals(object? obj) =>
			Equals(obj as TestClassGeneratorResult);

		/// <inheritdoc/>
		public bool Equals(TestClassGeneratorResult? other) =>
			other is not null &&
			base.Equals(other) &&
			ComparerHelper.Equal(TestClass, other.TestClass) &&
			ComparerHelper.Equal(TestMethods, other.TestMethods);

		/// <inheritdoc/>
		public override int GetHashCode() =>
			HashCodeHelper.Extend(base.GetHashCode()).With(TestClass).With(TestMethods);
	}
}
