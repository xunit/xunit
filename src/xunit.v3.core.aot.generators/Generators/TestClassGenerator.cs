using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class TestClassGenerator : XunitGenerator
{
	static readonly HashSet<string> validReturnTypes = ["void", Types.System.Threading.Tasks.Task, Types.System.Threading.Tasks.ValueTask];
	readonly Dictionary<string, Func<INamedTypeSymbol, MethodDeclarationSyntax, IMethodSymbol, AttributeData, FactMethodRegistration?>> registrarsByAttribute = new()
	{
		[Types.Xunit.FactAttribute] = FactRegistrar.GetRegistration,
		[Types.Xunit.CulturedFactAttribute] = CulturedFactRegistrar.GetRegistration,
		[Types.Xunit.TheoryAttribute] = TheoryRegistrar.GetRegistration,
		[Types.Xunit.CulturedTheoryAttribute] = CulturedTheoryRegistrar.GetRegistration,
	};

	protected override sealed void Initialize(
		IncrementalGeneratorInitializationContext context,
		IncrementalValueProvider<string> projectPath,
		IncrementalValueProvider<INamedTypeSymbol> objectType)
	{
		var result =
			context
				.SyntaxProvider
				.CreateSyntaxProvider(
					(syntaxNode, cancellationToken) => syntaxNode is ClassDeclarationSyntax,
					Transform
				)
				.WhereNotNull()
				.Combine(projectPath)
				.Select((pair, _) =>
				{
					pair.Left.ProjectPath = pair.Right;
					return pair.Left;
				});

		context.RegisterSourceOutput(result, Register);
	}

	void ProcessTestClass(
		SemanticModel semanticModel,
		ClassDeclarationSyntax classDeclaration,
		INamedTypeSymbol classSymbol,
		TestClassGeneratorResult result,
		CancellationToken cancellationToken)
	{
		// We need to process the base class, but only if it's part of the current declaration
		if (classDeclaration.BaseList is null)
			return;

		if (classDeclaration.BaseList.Types.FirstOrDefault()?.Type is not SimpleNameSyntax baseClassIdentifier)
			return;

		var baseClassSymbol = default(INamedTypeSymbol);

		try
		{
			baseClassSymbol = semanticModel.GetSymbolInfo(baseClassIdentifier, cancellationToken).Symbol as INamedTypeSymbol;
			if (baseClassSymbol is null)
				return;
		}
		catch
		{
			// Sometimes this throws because the base class isn't defined in source
			return;
		}

		foreach (var baseClassDeclaration in baseClassSymbol.DeclaringSyntaxReferences.Select(sr => sr.GetSyntax(cancellationToken)).OfType<ClassDeclarationSyntax>())
		{
			// We get methods from the symbol for base types, because now we don't care where they're defined; we know
			// we've gated on them just the single time by virtue of the declaration-based BaseList usage.
			foreach (var baseClassMethodSymbol in baseClassSymbol.GetMembers().OfType<IMethodSymbol>())
				foreach (var baseClassMethodDeclaration in baseClassMethodSymbol.DeclaringSyntaxReferences.Select(sr => sr.GetSyntax(cancellationToken)).OfType<MethodDeclarationSyntax>())
					ProcessTestMethod(classSymbol, baseClassMethodDeclaration, baseClassMethodSymbol, result);

			ProcessTestClass(semanticModel, baseClassDeclaration, classSymbol, result, cancellationToken);
		}
	}

	void ProcessTestMethod(
		INamedTypeSymbol classSymbol,
		MethodDeclarationSyntax methodDeclaration,
		IMethodSymbol methodSymbol,
		TestClassGeneratorResult result)
	{
		if (methodSymbol.DeclaredAccessibility != Accessibility.Public || methodSymbol.IsAbstract)
			return;

		var attributes =
			(from attr in methodSymbol.GetAttributes()
			 let attrType = attr.AttributeClass?.ToCSharp(includeGlobal: false)
			 where attrType is not null
			 select registrarsByAttribute.TryGetValue(attrType, out var registrar) ? (attr, registrar) : (null, null))
			.Where(x => x.attr is not null && x.registrar is not null)
			.ToImmutableArray();

		if (attributes.Length != 1)
			return;

		var overloads = classSymbol.GetAllMembers(methodSymbol.Name);
		if (overloads.Length > 1)
			return;

		if (methodSymbol is { ReturnsVoid: true, IsAsync: true })
			return;

		if (!validReturnTypes.Contains(methodSymbol.ReturnType.ToCSharp(includeGlobal: false)))
			return;

		var (attribute, registrar) = attributes[0];
		var registration = registrar(classSymbol, methodDeclaration, methodSymbol, attribute);
		if (registration is not null)
			result
				.TestMethods
				.AddOrGet(registration.MethodName, () => (registration.TestMethod, []))
				.TestCaseFactories
				.Add(registration.TestCaseFactory);
	}

	void Register(
		SourceProductionContext context,
		TestClassGeneratorResult result)
	{
		if (result.TestClass is null || result.TestMethods.Count == 0)
			return;

		var initialization = new StringBuilder();
		initialization.Append($$"""
			global::Xunit.v3.RegisteredEngineConfig.RegisterCodeGenTestClass({{result.TestClassType.Quoted()}}, {{result.TestClass.ToGeneratedInit()}});

			""");

		if (result.TestClass.Traits is not null)
			foreach (var trait in result.TestClass.Traits)
				initialization.Append($$"""
					global::Xunit.v3.RegisteredEngineConfig.RegisterCodeGenTestClassTrait({{result.TestClassType.Quoted()}}, {{trait.Key.Quoted()}}, {{string.Join(", ", trait.Value.Select(v => v.Quoted()))}});

					""");

		foreach (var kvp in result.TestMethods)
		{
			initialization.Append($$"""
				global::Xunit.v3.RegisteredEngineConfig.RegisterCodeGenTestMethod({{result.TestClassType.Quoted()}}, {{kvp.Key.Quoted()}}, {{kvp.Value.TestMethod.ToGeneratedInit()}});

				""");

			if (kvp.Value.TestMethod.Traits is not null)
				foreach (var trait in kvp.Value.TestMethod.Traits)
					initialization.Append($$"""
						global::Xunit.v3.RegisteredEngineConfig.RegisterCodeGenTestMethodTrait({{result.TestClassType.Quoted()}}, {{kvp.Key.Quoted()}}, {{trait.Key.Quoted()}}, {{string.Join(", ", trait.Value.Select(v => v.Quoted()))}});

						""");

			foreach (var testCaseFactory in kvp.Value.TestCaseFactories)
				initialization.Append($$"""
					global::Xunit.v3.RegisteredEngineConfig.RegisterCodeGenTestCaseFactory({{result.TestClassType.Quoted()}}, {{kvp.Key.Quoted()}}, {{testCaseFactory}});

					""");
		}

		AddInitAttribute(context, result, initialization.ToString());
	}

	TestClassGeneratorResult? Transform(
		GeneratorSyntaxContext context,
		CancellationToken cancellationToken)
	{
		if (context.Node is not ClassDeclarationSyntax classDeclaration)
			return null;
		if (context.SemanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken) is not INamedTypeSymbol classSymbol)
			return null;

		var result = new TestClassGeneratorResult(context)
		{
			GeneratorSuffix = classSymbol.Name + "٠",
			TestClassType = classSymbol.ToCSharp(),
		};

		// For the discovered class declaration, we only want to do methods defined in the current class declaration.
		// Other parts of partials will get their own registration based on their declaration.
		foreach (var methodDeclaration in classDeclaration.ChildNodes().OfType<MethodDeclarationSyntax>())
			if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken) is IMethodSymbol methodSymbol)
				ProcessTestMethod(classSymbol, methodDeclaration, methodSymbol, result);

		ProcessTestClass(context.SemanticModel, classDeclaration, classSymbol, result, cancellationToken);

		if (result.TestMethods.Count == 0 || classSymbol.DeclaredAccessibility != Accessibility.Public)
			return null;

		for (var containingType = classSymbol.ContainingType; containingType is not null; containingType = containingType.ContainingType)
			if (containingType.IsGenericType)
				return null;

		if (classSymbol.AllInterfaces.Any(i => i.IsGeneric(Types.Xunit.ICollectionFixtureOfT)))
			return null;

		var classFixtures = new List<(string Type, string Factory)>();

		foreach (var classFixtureInterface in classSymbol.AllInterfaces.Where(i => i.IsGeneric(Types.Xunit.IClassFixtureOfT)))
			if (classFixtureInterface.TypeArguments[0] is INamedTypeSymbol fixtureType)
			{
				var fixtureFactory = CodeGenRegistration.ToFixtureFactory(fixtureType, "Class fixture type");
				if (fixtureFactory is not null)
					classFixtures.Add((fixtureType.ToCSharp(), fixtureFactory));
			}

		var testCaseOrdererFactory = default(string);
		var testMethodOrdererFactory = default(string);
		var traits = default(Dictionary<string, HashSet<string>>);

		foreach (var classAttribute in classSymbol.GetAttributes())
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

		result.TestClass = new CodeGenTestClassRegistration()
		{
			Class = classSymbol.ToCSharp(),
			ClassFactory = CodeGenRegistration.ToObjectFactory(classSymbol, "Test class"),
			ClassFixtures = classFixtures,
			TestCaseOrdererFactory = testCaseOrdererFactory,
			TestMethodOrdererFactory = testMethodOrdererFactory,
			Traits = traits,
		};

		return result;
	}
}
