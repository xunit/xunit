using System.Text;
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
		if (result is null || !result.ShouldGenerate || result.Registration is null)
			return;

		var builder = new StringBuilder();
		result.Registration.GenerateSource(builder);

		AddInitAttribute(context, result, builder.ToString());
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

		var result = new GeneratorResult(context)
		{
			GeneratorSuffix = context.TargetSymbol.Name + "٠",
			Registration = new(name, context.TargetSymbol as INamedTypeSymbol),
		};

		if (attribute.NamedArguments.FirstOrDefault(kvp =>
				kvp.Key == Names.CollectionDefinitionAttribute.DisableParallelization) is
			{
				Value.Value: true
			})
			result.Registration.DisableParallelization = true;

		if (attribute.NamedArguments.FirstOrDefault(kvp =>
				kvp.Key == Names.CollectionDefinitionAttribute.ParallelismOptions) is
			{ Value.Value: IConvertible parallelismOptions })
			result.Registration.ParallelismOptions = Convert.ToInt32(parallelismOptions, CultureInfo.InvariantCulture);

		if (context.TargetSymbol is ITypeSymbol targetType)
		{
			if (!targetType.IsSafeToReference())
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
					result.Registration.TestCaseOrdererFactory = classAttribute.ToOrdererFactory(Types.Xunit.v3.ITestCaseOrderer);
					break;

				case Types.Xunit.TestClassOrdererAttribute:
				case Types.Xunit.TestClassOrdererAttribute + "<>":
					result.Registration.TestClassOrdererFactory = classAttribute.ToOrdererFactory(Types.Xunit.v3.ITestClassOrderer);
					break;

				case Types.Xunit.TestMethodOrdererAttribute:
				case Types.Xunit.TestMethodOrdererAttribute + "<>":
					result.Registration.TestMethodOrdererFactory = classAttribute.ToOrdererFactory(Types.Xunit.v3.ITestMethodOrderer);
					break;
			}
		}

		if (context.TargetSymbol is INamedTypeSymbol namedTargetSymbol)
			foreach (var interfaceSymbol in namedTargetSymbol.AllInterfaces.Where(i => i.IsGenericType))
				switch (interfaceSymbol.ConstructUnboundGenericType().ToCSharp(includeGlobal: false))
				{
					case Types.Xunit.IClassFixtureOfT:
						addFixture(result.Registration.AddClassFixture, interfaceSymbol);
						break;

					case Types.Xunit.ICollectionFixtureOfT:
						addFixture(result.Registration.AddCollectionFixture, interfaceSymbol);
						break;
				}

		return result;

		static void addFixture(
			Action<INamedTypeSymbol> registrar,
			INamedTypeSymbol interfaceSymbol)
		{
			if (interfaceSymbol.TypeArguments.Length != 1)
				return;

			if (interfaceSymbol.TypeArguments[0] is INamedTypeSymbol fixtureType)
				registrar(fixtureType);
		}
	}

	public sealed class GeneratorResult(GeneratorAttributeSyntaxContext context) :
		XunitGeneratorResult(context.SemanticModel.SyntaxTree.FilePath, context.TargetNode.GetLocation()), IEquatable<GeneratorResult?>
	{
		public CodeGenTestCollectionRegistration? Registration { get; set; }

		public override bool Equals(object? obj) =>
			Equals(obj as GeneratorResult);

		public bool Equals(GeneratorResult? other) =>
			other is not null &&
			base.Equals(other) &&
			ComparerHelper.Equal(Registration, other.Registration);

		public override int GetHashCode() =>
			HashCodeHelper.Extend(base.GetHashCode()).With(Registration);
	}
}
