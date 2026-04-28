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

		var code = new List<string>
		{
			$$"""
			global::Xunit.v3.RegisteredEngineConfig.RegisterCollectionDefinition({{result.Name.Quoted()}}, {{result.Registration.ToGeneratedInit()}});
			"""
		};

		if (result.Registration.Traits is not null)
		{
			var name = result.Name.Quoted();
			var type =
				result.Registration.Type is null
					? "null"
					: $"typeof({result.Registration.Type})";

			foreach (var trait in result.Registration.Traits)
				code.Add($"""
					global::Xunit.v3.RegisteredEngineConfig.RegisterCodeGenTestCollectionTrait({name}, {type}, {trait.Key.Quoted()}, {string.Join(", ", trait.Value.Select(v => v.Quoted()))});
					""");
		}

		AddInitAttribute(context, result, string.Join("\n", code));
	}

	protected override sealed GeneratorResult? Transform(
		GeneratorAttributeSyntaxContext context,
		CancellationToken cancellationToken)
	{
		var attribute = context.Attributes.FirstOrDefault();
		if (attribute is null)
			return null;

		if (context.TargetSymbol.DeclaredAccessibility != Accessibility.Public)
			return null;

		var type = context.TargetSymbol.ToCSharp();

		var name = default(string);
		if (attribute.ConstructorArguments.Length == 1 && attribute.ConstructorArguments[0].Value is string stringValue)
			name = stringValue;

		var disableParallelization = false;
		if (attribute.NamedArguments.FirstOrDefault(kvp => kvp.Key == Names.Xunit.CollectionDefinitionAttribute.DisableParallelization) is { } namedArg
				&& namedArg.Value.Value is true)
			disableParallelization = true;

		var testCaseOrdererFactory = default(string);
		var testClassOrdererFactory = default(string);
		var testMethodOrdererFactory = default(string);
		var traits = default(Dictionary<string, HashSet<string>>);

		var result = new GeneratorResult(context)
		{
			GeneratorSuffix = context.TargetSymbol.Name + "٠",
			Name = name,
		};

		if (context.TargetSymbol is ITypeSymbol targetType)
		{
			var openGenericTypeParameter = targetType.RecursiveGetOpenGenericTypeParameter();
			if (openGenericTypeParameter is not null)
				return null;
		}

		foreach (var classAttribute in context.TargetSymbol.GetAttributes())
		{
			var attributeType =
				classAttribute.AttributeClass?.IsGenericType == true
					? classAttribute.AttributeClass.ConstructUnboundGenericType().ToString()
					: classAttribute.AttributeClass?.ToString();

			switch (attributeType)
			{
				case Types.Xunit.TestCaseOrdererAttribute:
				case Types.Xunit.TestCaseOrdererAttribute + "<>":
					testCaseOrdererFactory = CodeGenRegistration.ToOrdererFactory(classAttribute, Types.Xunit.v3.ITestCaseOrderer);
					break;

				case Types.Xunit.TestClassOrdererAttribute:
				case Types.Xunit.TestClassOrdererAttribute + "<>":
					testClassOrdererFactory = CodeGenRegistration.ToOrdererFactory(classAttribute, Types.Xunit.v3.ITestClassOrderer);
					break;

				case Types.Xunit.TestMethodOrdererAttribute:
				case Types.Xunit.TestMethodOrdererAttribute + "<>":
					testMethodOrdererFactory = CodeGenRegistration.ToOrdererFactory(classAttribute, Types.Xunit.v3.ITestMethodOrderer);
					break;

				case Types.Xunit.TraitAttribute:
					if (classAttribute.ConstructorArguments.Length == 2
							&& classAttribute.ConstructorArguments[0].Kind == TypedConstantKind.Primitive
							&& classAttribute.ConstructorArguments[1].Kind == TypedConstantKind.Primitive
							&& classAttribute.ConstructorArguments[0].Value is string traitName
							&& classAttribute.ConstructorArguments[1].Value is string traitValue)
					{
						traits ??= new(StringComparer.Ordinal);
						traits.Add(traitName, traitValue);
					}
					break;
			}
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
			TestCaseOrdererFactory = testCaseOrdererFactory,
			TestClassOrdererFactory = testClassOrdererFactory,
			TestMethodOrdererFactory = testMethodOrdererFactory,
			Traits = traits,
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
				return;

			var factory = CodeGenRegistration.ToFixtureFactory(fixtureType, $"{fixtureCategory} fixture type");
			if (factory is not null)
				collection.Add((fixtureType.ToCSharp(), factory));
		}
	}

	public sealed class GeneratorResult(GeneratorAttributeSyntaxContext context) :
		XunitGeneratorResult(context.SemanticModel, context.TargetNode), IEquatable<GeneratorResult?>
	{
		public string? Name { get; set; }

		public CodeGenTestCollectionRegistration? Registration { get; set; }

		public override bool Equals(object? obj) =>
			Equals(obj as GeneratorResult);

		public bool Equals(GeneratorResult? other) =>
			other is not null &&
			base.Equals(other) &&
			ComparerHelper.Equals(Name, other.Name) &&
			ComparerHelper.Equals(Registration, other.Registration);

		public override int GetHashCode() =>
			Hasher.Extend(base.GetHashCode()).With(Name).With(Registration);
	}
}
