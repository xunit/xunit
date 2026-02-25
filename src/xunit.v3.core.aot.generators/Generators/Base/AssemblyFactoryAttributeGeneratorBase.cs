using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

/// <summary>
/// A source generator which converts a single assembly attribute into an engine init attribute with
/// registration code.
/// </summary>
/// <param name="fullyQualifiedAttributeTypeName">The fully qualified attribute name
/// (e.g., <c>"Xunit.TestFrameworkAttribute"</c>)</param>
/// <param name="simpleAttributeName">The simple attribute name (e.g., <c>"TestFrameworkAttribute"</c>)</param>
/// <param name="factoryRegistrationMethod">The factory registration method name off <c>RegisteredEngineConfig</c>
/// (e.g., <c>"RegisterTestFrameworkFactory"</c>). This is required if you don't override <see cref="GetRegistration"/>,
/// but can be omitted if you do.</param>
/// <remarks>
/// This generator converts:<br />
/// <br />
/// <c>[assembly: Attribute(typeof(Implementation)]</c><br />
/// <br />
/// into an engine initialization attribute that calls:<br />
/// <br />
/// <c>RegisteredEngineConfig.FactoryProperty = (parameters) => new Implementation(parameters);</c>
/// </remarks>
public abstract class AssemblyFactoryAttributeGeneratorBase(
	string fullyQualifiedAttributeTypeName,
	string simpleAttributeName,
	string? factoryRegistrationMethod = null) :
		XunitAttributeGenerator<AssemblyFactoryAttributeGeneratorBase.GeneratorResult>(fullyQualifiedAttributeTypeName)
{
	protected override sealed void CreateSource(
		SourceProductionContext context,
		GeneratorResult result)
	{
		if (result is null || result.Factory is null)
			return;

		AddInitAttribute(context, result, GetRegistration(result));
	}

	protected virtual string? GetFactory(
		INamedTypeSymbol type,
		Location? location,
		GeneratorResult result)
	{
		Guard.ArgumentNotNull(type);
		Guard.ArgumentNotNull(result);

		if (EnsureParameterlessPublicCtor(type, location, result, out var ctor) &&
			ValidateImplementationType(type, location, result))
		{
			var factory = CodeGenRegistration.ToObjectFactory(type, ctor);
			if (factory is not null)
				return $"() => {factory}";

			result.Diagnostics.Add(
				Diagnostic.Create(
					DiagnosticDescriptors.X9000_TypeMustHaveCorrectPublicConstructor,
					location,
					type.ToDisplayString(),
					string.Empty
				)
			);
		}

		return null;
	}

	protected virtual string GetRegistration(GeneratorResult result) =>
		$"global::Xunit.v3.RegisteredEngineConfig.{Guard.ArgumentNotNull(factoryRegistrationMethod)}({Guard.ArgumentNotNull(result).Factory});";

	protected override sealed GeneratorResult? Transform(
		GeneratorAttributeSyntaxContext context,
		CancellationToken cancellationToken)
	{
		if (context.TargetSymbol is not IAssemblySymbol)
			return null;

		var result = new GeneratorResult(context);

		var attribute = context.Attributes.FirstOrDefault();
		if (attribute is not null &&
			attribute.ConstructorArguments.Length == 1 &&
			attribute.ConstructorArguments[0].Value is INamedTypeSymbol type)
		{
			var location = attribute.ApplicationSyntaxReference.Location;
			var factory = GetFactory(type, location, result);
			if (factory is not null)
			{
				result.Type = type.ToCSharp();
				result.Factory = factory;
			}
		}

		return result;
	}

	protected virtual bool ValidateImplementationType(
		INamedTypeSymbol type,
		Location? location,
		GeneratorResult result) =>
			true;

	public sealed class GeneratorResult(GeneratorAttributeSyntaxContext context) :
		XunitGeneratorResult(context.SemanticModel, context.TargetNode)
	{
		public string? Factory { get; set; }

		public string? Type { get; set; }
	}
}
