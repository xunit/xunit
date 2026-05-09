using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

public abstract class IDAndTypeGenerator(
	string fullyQualifiedAttributeTypeName,
	Func<string, string, string> perItemInit) :
		XunitAttributeGenerator<IDAndTypeGenerator.GeneratorResult>(fullyQualifiedAttributeTypeName)
{
	protected override string BaseInitAttributeName =>
		"global::Xunit.Runner.Common.RunnerInitializationAttribute";

	protected override sealed void CreateSource(
		SourceProductionContext context,
		GeneratorResult result)
	{
		if (result is null || result.Entries.Count == 0)
			return;

		AddInitAttribute(
			context, result,
			string.Join("\n", result.Entries.Where(rw => rw.Type is not null).Select(rw => perItemInit(rw.ID, rw.Type!)))
		);
	}

	protected override sealed GeneratorResult? Transform(
		GeneratorAttributeSyntaxContext context,
		CancellationToken cancellationToken)
	{
		if (context.TargetSymbol is not IAssemblySymbol)
			return null;

		var result = new GeneratorResult(context);

		foreach (var attribute in context.Attributes)
		{
			var (type, id) = GetTypeAndID(attribute);
			if (type is not null)
			{
				if (type.HasParameterlessPublicCtor(out var _) && ValidateType(type))
					result.Entries.Add(new(id, type.ToString()));
			}
		}

		return result;
	}

	protected virtual (INamedTypeSymbol? Type, string ID) GetTypeAndID(AttributeData attribute) =>
		FullyQualifiedAttributeTypeName.EndsWith("`1", StringComparison.Ordinal)
			? GetTypeAndIDGeneric(attribute)
			: GetTypeAndIDNonGeneric(attribute);

	protected static (INamedTypeSymbol? Type, string ID) GetTypeAndIDGeneric(AttributeData attribute)
	{
		if (attribute?.AttributeClass is not { } attributeType)
			return (null, string.Empty);

		return
			attributeType.TypeArguments.Length == 1 &&
			attributeType.TypeArguments[0] is INamedTypeSymbol type &&
			attribute.ConstructorArguments.Length == 1 &&
			attribute.ConstructorArguments[0].Value is string id
				? (type, id)
				: (null, string.Empty);
	}

	protected static (INamedTypeSymbol? Type, string ID) GetTypeAndIDNonGeneric(AttributeData attribute) =>
		Guard.ArgumentNotNull(attribute).ConstructorArguments.Length == 2 &&
		attribute.ConstructorArguments[0].Value is string id &&
		attribute.ConstructorArguments[1].Value is INamedTypeSymbol type
			? (type, id)
			: (null, string.Empty);

	protected virtual bool ValidateType(INamedTypeSymbol type) =>
		true;

	public sealed class GeneratorResult(GeneratorAttributeSyntaxContext context) :
		XunitGeneratorResult(context.SemanticModel.SyntaxTree.FilePath, context.TargetNode.GetLocation()), IEquatable<GeneratorResult?>
	{
		public List<IDAndType> Entries = [];

		public override bool Equals(object? obj) =>
			Equals(obj as GeneratorResult);

		public bool Equals(GeneratorResult? other) =>
			other is not null &&
			base.Equals(other) &&
			ComparerHelper.Equal(Entries, other.Entries);

		public override int GetHashCode() =>
			HashCodeHelper.Extend(base.GetHashCode()).With(Entries);
	}

	public sealed class IDAndType : IEquatable<IDAndType>
	{
		public IDAndType(
			string id,
			string? type)
		{
			ID = id ?? throw new ArgumentNullException(nameof(id));
			Type = type;
		}

		public string ID { get; }

		public string? Type { get; }

		public override bool Equals(object? obj) =>
			Equals(obj as IDAndType);

		public bool Equals(IDAndType? other) =>
			other is not null &&
			ComparerHelper.Equal(ID, other.ID) &&
			ComparerHelper.Equal(Type, other.Type);

		public override int GetHashCode() =>
			HashCodeHelper.Start()
				.With(ID)
				.With(Type);
	}
}
