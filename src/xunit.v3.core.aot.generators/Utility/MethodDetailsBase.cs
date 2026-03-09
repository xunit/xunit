using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Generators;

public class MethodDetailsBase
{
	public MethodDetailsBase(
		INamedTypeSymbol classSymbol,
		MethodDeclarationSyntax methodDeclaration,
		IMethodSymbol methodSymbol,
		AttributeData attribute)
	{
		ClassSymbol = Guard.ArgumentNotNull(classSymbol);
		MethodDeclaration = Guard.ArgumentNotNull(methodDeclaration);
		MethodSymbol = Guard.ArgumentNotNull(methodSymbol);
		Attribute = Guard.ArgumentNotNull(attribute);

		if (Attribute.ConstructorArguments.Length > 1
			&& Attribute.ConstructorArguments[attribute.ConstructorArguments.Length - 2].Value is string sourceFilePathValue
			&& Attribute.ConstructorArguments[attribute.ConstructorArguments.Length - 1].Value is int sourceLineNumberValue)
		{
			SourceFilePath = sourceFilePathValue;
			SourceLineNumber = sourceLineNumberValue;
		}

		var containingType = methodSymbol.ContainingType;
		if (containingType is not null && !SymbolEqualityComparer.Default.Equals(containingType, classSymbol))
			DeclaredTypeIndex = containingType.ToCSharp();
	}

	public int Arity =>
		MethodDeclaration.Arity;

	public AttributeData Attribute { get; }

	public List<string> BeforeAfterTestAttributes { get; } = [];

	public INamedTypeSymbol ClassSymbol { get; }

	public string? DeclaredTypeIndex { get; }

	public List<Diagnostic> Diagnostics { get; } = [];

	public string? DisplayName { get; set; }

	public bool Explicit { get; set; }

	public MethodDeclarationSyntax MethodDeclaration { get; }

	public bool MethodIsStatic =>
		MethodSymbol.IsStatic;

	public string MethodName =>
		MethodSymbol.Name;

	public IMethodSymbol MethodSymbol { get; }

	public List<string> SkipExceptions { get; } = [];

	public string? SkipReason { get; set; }

	public INamedTypeSymbol? SkipType { get; set; }

	public string? SkipUnless { get; set; }

	public string? SkipWhen { get; set; }

	public string? SourceFilePath { get; set; }

	public int? SourceLineNumber { get; set; }

	public string? TestCaseOrderer { get; set; }

	public int Timeout { get; set; }

	public Dictionary<string, List<string>> Traits { get; } =
		new(StringComparer.OrdinalIgnoreCase);

	public virtual void Process()
	{
		foreach (var kvp in Attribute.NamedArguments)
			ProcessNamedArgument(kvp.Key, kvp.Value);

		foreach (var methodAttribute in MethodSymbol.GetAttributes())
		{
			var attributeTypeName = methodAttribute.AttributeClass?.ToCSharp(includeGlobal: false);
			if (attributeTypeName is null)
				continue;

			ProcessMethodAttribute(attributeTypeName, methodAttribute);
		}

		if (SkipUnless is not null && SkipWhen is not null)
		{
			Diagnostics.Add(
				Diagnostic.Create(
					DiagnosticDescriptors.X9006_CannotSetBothSkipUnlessAndSkipWhen,
					Attribute.ApplicationSyntaxReference?.Location
				)
			);
		}
		else
		{
			VerifySkipProperty(SkipUnless);
			VerifySkipProperty(SkipWhen);
		}
	}

	protected virtual void ProcessMethodAttribute(
		string typeName,
		AttributeData attribute)
	{
		Guard.ArgumentNotNull(typeName);
		Guard.ArgumentNotNull(attribute);

		if (typeName == Types.Xunit.TraitAttribute)
		{
			if (attribute.ConstructorArguments.Length == 2
					&& attribute.ConstructorArguments[0].Value is string key
					&& attribute.ConstructorArguments[1].Value is string value)
				Traits.AddOrGet(key).Add(value);
		}
		else if (typeName == Types.Xunit.TestCaseOrdererAttribute)
		{
			if (attribute.ConstructorArguments.Length == 1
					&& attribute.ConstructorArguments[0].Value is INamedTypeSymbol testCaseOrdererSymbol)
				TestCaseOrderer = testCaseOrdererSymbol.ToCSharp();
		}
		else if (attribute.AttributeClass.InheritsFrom(Types.Xunit.v3.BeforeAfterTestAttribute))
		{
			if (attribute.AttributeClass.RecursiveGetNonPublicNonInternalType() is null)
				BeforeAfterTestAttributes.Add(typeName);
			else
				Diagnostics.Add(
					Diagnostic.Create(
						DiagnosticDescriptors.X9004_TypeMustBePublicOrInternal,
						attribute.AttributeClass.Locations.FirstOrDefault(),
						"Attribute",
						attribute.AttributeClass.ToDisplayString()
					)
				);
		}
	}

	protected virtual void ProcessNamedArgument(
		string name,
		TypedConstant value)
	{
		switch (name)
		{
			case Names.Xunit.Internal.FactAttributeBase.DisplayName:
				DisplayName = value.Value as string;
				break;

			case Names.Xunit.Internal.FactAttributeBase.Explicit:
				Explicit = value.Value is true;
				break;

			case Names.Xunit.Internal.FactAttributeBase.Skip:
				SkipReason = value.Value as string;
				break;

			case Names.Xunit.Internal.FactAttributeBase.SkipType:
				SkipType = value.Value as INamedTypeSymbol;
				break;

			case Names.Xunit.Internal.FactAttributeBase.SkipUnless:
				SkipUnless = value.Value as string;
				break;

			case Names.Xunit.Internal.FactAttributeBase.SkipWhen:
				SkipWhen = value.Value as string;
				break;

			case Names.Xunit.Internal.FactAttributeBase.SkipExceptions:
				SkipExceptions.AddRange(ToTypeArray(value.Values));
				break;

			case Names.Xunit.Internal.FactAttributeBase.Timeout:
				if (value.Value is int timeoutValue)
					Timeout = timeoutValue;
				break;
		}
	}

	protected IEnumerable<string> ToTypeArray(ImmutableArray<TypedConstant> values)
	{
		foreach (var value in values)
			if (value.Value is INamedTypeSymbol typeValue)
				if (typeValue.RecursiveGetNonPublicNonInternalType() is null)
					yield return typeValue.ToCSharp();
				else
					Diagnostics.Add(
						Diagnostic.Create(
							DiagnosticDescriptors.X9004_TypeMustBePublicOrInternal,
							typeValue.Locations.FirstOrDefault(),
							"SkipExceptions",
							typeValue.ToDisplayString()
						)
					);
	}

	void VerifySkipProperty(string? propertyName)
	{
		if (propertyName is null)
			return;

		var currentSymbol = SkipType ?? ClassSymbol;

		while (currentSymbol is not null)
		{
			var property =
				currentSymbol
					.GetMembers()
					.OfType<IPropertySymbol>()
					.FirstOrDefault(symbol => symbol.Name == propertyName);

			if (property is not null)
			{
				if (property.DeclaredAccessibility == Accessibility.Public && property.Type.ToCSharp() == "bool")
					return;

				break;
			}

			currentSymbol = currentSymbol.BaseType;
		}

		Diagnostics.Add(
			Diagnostic.Create(
				DiagnosticDescriptors.X9002_TypeMustHaveStaticPublicProperty,
				Attribute.ApplicationSyntaxReference?.Location,
				SkipType ?? ClassSymbol,
				propertyName,
				"bool"
			)
		);
	}
}
