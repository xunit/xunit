using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class TestClassGenerator : XunitGenerator
{
	static readonly HashSet<string> validReturnTypes = ["void", Types.System.Threading.Tasks.Task, Types.System.Threading.Tasks.ValueTask];
	readonly Dictionary<string, Func<SemanticModel, INamedTypeSymbol, MethodDeclarationSyntax, IMethodSymbol, AttributeData, CodeGenTestMethodRegistration?>> registrarsByAttribute = new()
	{
		[Types.Xunit.FactAttribute] = FactRegistrar.GetRegistration,
		[Types.Xunit.CulturedFactAttribute] = CulturedFactRegistrar.GetRegistration,
		[Types.Xunit.TheoryAttribute] = TheoryRegistrar.GetRegistration,
		[Types.Xunit.CulturedTheoryAttribute] = CulturedTheoryRegistrar.GetRegistration,
	};

	protected override sealed void Initialize(
		IncrementalGeneratorInitializationContext context,
		IncrementalValueProvider<XunitMSBuildProperties> properties)
	{
		var result =
			context
				.SyntaxProvider
				.CreateSyntaxProvider(
					(syntaxNode, cancellationToken) => syntaxNode is ClassDeclarationSyntax,
					Transform
				)
				.WhereNotNull()
				.Combine(properties)
				.Select((pair, _) =>
				{
					pair.Left.Initialize(pair.Right);
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
					ProcessTestMethod(semanticModel, classSymbol, baseClassMethodDeclaration, baseClassMethodSymbol, result);

			ProcessTestClass(semanticModel, baseClassDeclaration, classSymbol, result, cancellationToken);
		}
	}

	void ProcessTestMethod(
		SemanticModel semanticModel,
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
		var registration = registrar(semanticModel, classSymbol, methodDeclaration, methodSymbol, attribute);
		if (registration is not null)
			result.TestMethods.Add(registration);
	}

	void Register(
		SourceProductionContext context,
		TestClassGeneratorResult result)
	{
		if (!result.ShouldGenerate || result.TestClass is null || result.TestMethods.Count == 0)
			return;

		var initialization = new StringBuilder();

		result.TestClass.GenerateSource(initialization);
		foreach (var testMethod in result.TestMethods)
			testMethod.GenerateSource(initialization);

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
				ProcessTestMethod(context.SemanticModel, classSymbol, methodDeclaration, methodSymbol, result);

		ProcessTestClass(context.SemanticModel, classDeclaration, classSymbol, result, cancellationToken);

		if (result.TestMethods.Count == 0 || classSymbol.DeclaredAccessibility != Accessibility.Public)
			return null;

		for (var containingType = classSymbol.ContainingType; containingType is not null; containingType = containingType.ContainingType)
			if (containingType.IsGenericType)
				return null;

		if (classSymbol.AllInterfaces.Any(i => i.IsGeneric(Types.Xunit.ICollectionFixtureOfT)))
			return null;

		result.TestClass = new CodeGenTestClassRegistration(classSymbol);

		foreach (var classFixtureInterface in classSymbol.AllInterfaces.Where(i => i.IsGeneric(Types.Xunit.IClassFixtureOfT)))
			if (classFixtureInterface.TypeArguments[0] is INamedTypeSymbol fixtureType)
				result.TestClass.AddClassFixture(fixtureType);

		// We want only the locally defined traits, so we don't double up when there are partials
		foreach (var attributeSyntax in classDeclaration.AttributeLists.SelectMany(a => a.Attributes))
		{
			var attributeSymbol = context.SemanticModel.GetTypeInfo(attributeSyntax, cancellationToken);
			if (attributeSymbol.Type?.ToString() == Types.Xunit.TraitAttribute
					&& attributeSyntax.ArgumentList?.Arguments.Count == 2
					&& attributeSyntax.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax nameExpression
					&& attributeSyntax.ArgumentList.Arguments[1].Expression is LiteralExpressionSyntax valueExpression)
				result.TestClass.AddTrait(nameExpression.Token.ValueText, valueExpression.Token.ValueText);
		}

		// We want orderers no matter where they're defined
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
					result.TestClass.TestCaseOrdererFactory = classAttribute.ToOrdererFactory(Types.Xunit.v3.ITestCaseOrderer);
					break;

				case Types.Xunit.TestMethodOrdererAttribute:
				case Types.Xunit.TestMethodOrdererAttribute + "<>":
					result.TestClass.TestMethodOrdererFactory = classAttribute.ToOrdererFactory(Types.Xunit.v3.ITestMethodOrderer);
					break;
			}
		}

		return result;
	}
}
