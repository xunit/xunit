using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

public class CaptureOutputGeneratorBase(
	string fullyQualifiedAttributeTypeName,
	string fixtureTypeName) :
		XunitAssemblyAttributeGenerator(fullyQualifiedAttributeTypeName)
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		CodeGenTestAssemblyRegistration registration,
		AttributeData attribute) =>
			(registration ?? throw new ArgumentNullException(nameof(registration))).AddAssemblyFixture(fixtureTypeName);
}
