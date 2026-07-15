using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class ParallelizationAttributeGenerator() :
	XunitAssemblyAttributeGenerator(Types.Xunit.v3.ParallelizationAttribute)
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		CodeGenTestAssemblyRegistration registration,
		AttributeData attribute) =>
			Guard.ArgumentNotNull(registration).SetParallelization(attribute);
}
