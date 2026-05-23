#nullable enable

#pragma warning disable IDE0290 // Use primary constructor

using System;

namespace Xunit.Generators
{
	/// <summary>
	/// Used to decorate implementations of <see cref="TestClassGenerator"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public sealed class TestMethodGeneratorAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestMethodGeneratorAttribute"/> class.
		/// </summary>
		/// <param name="fullyQualifiedAttributeType">The fully qualified attribute type name that signifies
		/// a test method</param>
		public TestMethodGeneratorAttribute(string fullyQualifiedAttributeType) =>
			FullyQualifiedAttributeType = fullyQualifiedAttributeType ?? throw new ArgumentNullException(nameof(fullyQualifiedAttributeType));

		/// <summary>
		/// Gets the fully qualified attribute type name that decorates a test method
		/// </summary>
		public string FullyQualifiedAttributeType { get; }
	}
}
