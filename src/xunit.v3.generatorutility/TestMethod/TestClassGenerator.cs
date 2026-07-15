#nullable enable

#pragma warning disable IDE0028 // Simplify collection initialization

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Generators
{
	/// <summary>
	/// A source generator which registers test classes, by using implementations of
	/// <see cref="ITestMethodGenerator"/> which are found dynamically by being decorated
	/// with <see cref="TestMethodGeneratorAttribute"/>.
	/// </summary>
	[Generator(LanguageNames.CSharp)]
	public class TestClassGenerator : XunitGenerator
	{
		static readonly Dictionary<string, ITestMethodGenerator> testMethodGenerators;

		static TestClassGenerator()
		{
			testMethodGenerators = new Dictionary<string, ITestMethodGenerator>();

			var generatorRegistrations =
				from exportedType in typeof(TestClassGenerator).Assembly.GetExportedTypes()
				let attribute = exportedType.GetCustomAttribute<TestMethodGeneratorAttribute>()
				where attribute != null
				select (attributeType: attribute.FullyQualifiedAttributeType, generatorType: exportedType);

			foreach (var generatorRegistration in generatorRegistrations)
			{
				try
				{
					if (Activator.CreateInstance(generatorRegistration.generatorType) is ITestMethodGenerator generator)
						testMethodGenerators[generatorRegistration.attributeType] = generator;
				}
				catch { }
			}
		}

		/// <inheritdoc/>
		protected override sealed void Initialize(
			IncrementalGeneratorInitializationContext context,
			IncrementalValueProvider<XunitMSBuildProperties> properties)
		{
			if (testMethodGenerators.Count == 0)
				return;

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

		static void ProcessTestClass(
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

		static void ProcessTestMethod(
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
				 let attrType = attr.AttributeClass?.ToCSharp(includeGlobal: false, asOpenGeneric: true)
				 where attrType is not null
				 select testMethodGenerators.TryGetValue(attrType, out var generator) ? (attr, generator) : (null, null))
				.Where(x => x.attr is not null && x.generator is not null)
				.ToImmutableArray();

			if (attributes.Length != 1)
				return;

			var overloads = classSymbol.GetAllMembers(methodSymbol.Name);
			if (overloads.Length > 1)
				return;

			if (methodSymbol is { ReturnsVoid: true, IsAsync: true })
				return;

			var (attribute, generator) = attributes[0];
			var registration = generator.GetTestMethodRegistration(semanticModel, classSymbol, methodDeclaration, methodSymbol, attribute);
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

			var result = new TestClassGeneratorResult(context) { GeneratorSuffix = classSymbol.Name + "٠" };

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

			var allInterfacesByTypeName =
				classSymbol
					.AllInterfaces
					.ToMultiValueDictionary(i => i.ToCSharp(includeGlobal: false, asOpenGeneric: true));

			// ICollectionFixture<T> is not valid on a test class
			if (allInterfacesByTypeName.ContainsKey(Types.Xunit.ICollectionFixtureOfT))
				return null;

			result.TestClass = new CodeGenTestClassRegistration(classSymbol);

			if (allInterfacesByTypeName.TryGetValue(Types.Xunit.IClassFixtureOfT, out var classFixtureInterfaces))
				foreach (var classFixtureInterface in classFixtureInterfaces)
					if (classFixtureInterface.TypeArguments[0] is INamedTypeSymbol fixtureType)
						result.TestClass.AddClassFixture(fixtureType);

			foreach (var classAttribute in classSymbol.GetAttributes())
			{
				var attributeType =
					classAttribute.AttributeClass?.IsGenericType == true
						? classAttribute.AttributeClass.ConstructUnboundGenericType().ToString()
						: classAttribute.AttributeClass?.ToString();

				switch (attributeType)
				{
					case Types.Xunit.TestClassAttribute:
						if (classAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == Names.TestClassAttribute.DisableParallelization) is { } namedArg
								&& namedArg.Value.Kind == TypedConstantKind.Primitive
								&& namedArg.Value.Value is true)
							result.TestClass.DisableParallelization = true;
						break;

					case Types.Xunit.TestCaseOrdererAttribute:
					case Types.Xunit.TestCaseOrdererAttributeOfT:
						result.TestClass.TestCaseOrdererFactory = classAttribute.ToOrdererFactory(Types.Xunit.v3.ITestCaseOrderer);
						break;

					case Types.Xunit.TestMethodOrdererAttribute:
					case Types.Xunit.TestMethodOrdererAttributeOfT:
						result.TestClass.TestMethodOrdererFactory = classAttribute.ToOrdererFactory(Types.Xunit.v3.ITestMethodOrderer);
						break;
				}
			}

			return result;
		}
	}
}
