using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class AssemblyFixtureAttributeGenerator() :
	XunitAssemblyAttributeGenerator(Types.Xunit.AssemblyFixtureAttribute)
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		CodeGenTestAssemblyRegistration registration,
		AttributeData attribute) =>
			(registration ?? throw new ArgumentNullException(nameof(registration))).AddAssemblyFixture(attribute);
}

[Generator(LanguageNames.CSharp)]
public class AssemblyFixtureAttributeOfTGenerator() :
	XunitAssemblyAttributeGenerator(Types.Xunit.AssemblyFixtureAttribute + "`1")
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		CodeGenTestAssemblyRegistration registration,
		AttributeData attribute) =>
			(registration ?? throw new ArgumentNullException(nameof(registration))).AddAssemblyFixture(attribute);
}
