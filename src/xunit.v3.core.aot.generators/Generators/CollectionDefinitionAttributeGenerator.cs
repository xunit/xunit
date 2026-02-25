using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class CollectionDefinitionAttributeGenerator() :
	XunitAttributeGenerator<CollectionDefinitionAttributeGenerator.GeneratorResult>(Types.Xunit.CollectionDefinitionAttribute)
{
	protected override sealed void CreateSource(
		SourceProductionContext context,
		GeneratorResult result)
	{
		if (result is null || result.Registration is null)
			return;

		AddInitAttribute(
			context, result,
			$$"""
			global::Xunit.v3.RegisteredEngineConfig.RegisterCollectionDefinition({{result.Name.Quoted()}}, {{result.Registration.ToGeneratedInit()}});
			"""
		);
	}

	protected override sealed GeneratorResult? Transform(
		GeneratorAttributeSyntaxContext context,
		CancellationToken cancellationToken)
	{
		var attribute = context.Attributes.FirstOrDefault();
		if (attribute is null)
			return null;

		var type = context.TargetSymbol.ToCSharp();

		var name = default(string);
		if (attribute.ConstructorArguments.Length == 1 && attribute.ConstructorArguments[0].Value is string stringValue)
			name = stringValue;

		var disableParallelization = false;
		if (attribute.NamedArguments.FirstOrDefault(kvp => kvp.Key == Names.Xunit.CollectionDefinitionAttribute.DisableParallelization) is { } namedArg
				&& namedArg.Value.Value is true)
			disableParallelization = true;

		var testCaseOrdererType = default(string);
		var testClassOrdererType = default(string);
		var testMethodOrdererType = default(string);
		var result = new GeneratorResult(context)
		{
			GeneratorSuffix = context.TargetSymbol.Name + "٠",
			Name = name,
		};

		if (context.TargetSymbol.DeclaredAccessibility != Accessibility.Public)
		{
			result.Diagnostics.Add(
				Diagnostic.Create(
					DiagnosticDescriptors.X1027_CollectionDefinitionClassMustBePublic,
					context.TargetSymbol.Locations.FirstOrDefault()
				)
			);

			return result;
		}

		if (context.TargetSymbol is ITypeSymbol targetType)
		{
			var openGenericTypeParameter = targetType.RecursiveGetOpenGenericTypeParameter();
			if (openGenericTypeParameter is not null)
			{
				result.Diagnostics.Add(
					Diagnostic.Create(
						DiagnosticDescriptors.X9005_GenericCollectionDefinitionNotSupported,
						openGenericTypeParameter.Locations.FirstOrDefault()
					)
				);

				return result;
			}
		}

		foreach (var classAttribute in context.TargetSymbol.GetAttributes())
			switch (classAttribute.AttributeClass?.ToString())
			{
				case Types.Xunit.TestCaseOrdererAttribute:
					testCaseOrdererType = toOrdererType(classAttribute, Types.Xunit.v3.ITestCaseOrderer);
					break;

				case Types.Xunit.TestClassOrdererAttribute:
					testClassOrdererType = toOrdererType(classAttribute, Types.Xunit.v3.ITestClassOrderer);
					break;

				case Types.Xunit.TestMethodOrdererAttribute:
					testMethodOrdererType = toOrdererType(classAttribute, Types.Xunit.v3.ITestMethodOrderer);
					break;
			}

		var classFixtures = new List<(string, string)>();
		var collectionFixtures = new List<(string, string)>();

		if (context.TargetSymbol is INamedTypeSymbol namedTargetSymbol)
			foreach (var interfaceSymbol in namedTargetSymbol.AllInterfaces.Where(i => i.IsGenericType))
				switch (interfaceSymbol.ConstructUnboundGenericType().ToCSharp(includeGlobal: false))
				{
					case Types.Xunit.IClassFixtureOfT:
						generateFactory(classFixtures, interfaceSymbol, "Class", context.TargetSymbol.Locations.FirstOrDefault());
						break;

					case Types.Xunit.ICollectionFixtureOfT:
						generateFactory(collectionFixtures, interfaceSymbol, "Collection", context.TargetSymbol.Locations.FirstOrDefault());
						break;
				}

		result.Registration = new CodeGenTestCollectionRegistration()
		{
			ClassFixtures = classFixtures,
			CollectionFixtures = collectionFixtures,
			DisableParallelization = disableParallelization,
			TestCaseOrdererType = testCaseOrdererType,
			TestClassOrdererType = testClassOrdererType,
			TestMethodOrdererType = testMethodOrdererType,
			Type = type,
		};

		return result;

		void generateFactory(
			List<(string, string)> collection,
			INamedTypeSymbol interfaceSymbol,
			string fixtureCategory,
			Location? location)
		{
			if (interfaceSymbol.TypeArguments.Length != 1)
				return;

			if (interfaceSymbol.TypeArguments[0] is not INamedTypeSymbol fixtureType)
				return;

			var nonPublicType = fixtureType.RecursiveGetNonPublicNonInternalType();
			if (nonPublicType is not null)
			{
				result.Diagnostics.Add(
					Diagnostic.Create(
						DiagnosticDescriptors.X9004_TypeMustBePublicOrInternal,
						nonPublicType.Locations.FirstOrDefault(),
						"Fixture",
						nonPublicType.ToDisplayString()
					)
				);
				return;
			}

			var factory = CodeGenRegistration.ToFixtureFactory(
				fixtureType,
				location,
				result,
				$"{fixtureCategory} fixture type",
				"global::Xunit.v3.FixtureMappingManager.TryGetFixtureArgument<{0}>(mappingManager)"
			);

			if (factory is not null)
				collection.Add((fixtureType.ToCSharp(), factory));
		}

		string? toOrdererType(
			AttributeData attribute,
			string requiredInterface)
		{
			if (attribute.ConstructorArguments.Length != 1 || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol ordererType)
				return null;

			var location = attribute.ApplicationSyntaxReference.Location;
			if (!EnsureImplementsInterface(ordererType, location, result, requiredInterface))
				return null;

			return ordererType.ToCSharp();
		}
	}

	public sealed class GeneratorResult(GeneratorAttributeSyntaxContext context) :
		XunitGeneratorResult(context.SemanticModel, context.TargetNode)
	{
		public string? Name { get; set; }

		public CodeGenTestCollectionRegistration? Registration { get; set; }
	}
}
