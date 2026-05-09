using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class TestPipelineStartupAttributeGenerator() :
	XunitAssemblyAttributeGenerator(Types.Xunit.v3.TestPipelineStartupAttribute)
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		CodeGenTestAssemblyRegistration registration,
		AttributeData attribute) =>
			(registration ?? throw new ArgumentNullException(nameof(registration))).SetTestPipelineStartup(attribute);
}

[Generator(LanguageNames.CSharp)]
public class TestPipelineStartupAttributeOfTGenerator() :
	XunitAssemblyAttributeGenerator(Types.Xunit.v3.TestPipelineStartupAttribute + "`1")
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		CodeGenTestAssemblyRegistration registration,
		AttributeData attribute) =>
			(registration ?? throw new ArgumentNullException(nameof(registration))).SetTestPipelineStartup(attribute);
}
