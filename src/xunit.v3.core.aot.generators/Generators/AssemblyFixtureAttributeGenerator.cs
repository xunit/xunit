using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class AssemblyFixtureAttributeGenerator() :
	AssemblyFactoryAttributeGeneratorBase(Types.Xunit.AssemblyFixtureAttribute)
{
	protected override string GetRegistration(
		string type,
		string factory) =>
			$"global::Xunit.v3.RegisteredEngineConfig.RegisterAssemblyFixtureFactory(typeof({Guard.ArgumentNotNull(type)}), async {Guard.ArgumentNotNull(factory)});";
}

[Generator(LanguageNames.CSharp)]
public class AssemblyFixtureAttributeOfTGenerator() :
	AssemblyFactoryAttributeGeneratorBase(Types.Xunit.AssemblyFixtureAttribute + "`1")
{
	protected override string GetRegistration(
		string type,
		string factory) =>
			$"global::Xunit.v3.RegisteredEngineConfig.RegisterAssemblyFixtureFactory(typeof({Guard.ArgumentNotNull(type)}), async {Guard.ArgumentNotNull(factory)});";
}
